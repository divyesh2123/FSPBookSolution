using FSPBook.Data;
using FSPBook.Repositroy.Implementations;
using FSPBook.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FSPBook.Tests.Repositories;

public class PostRepositoryTests
{
    private static readonly DateTimeOffset BaseDate = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Seeds two profiles and <paramref name="postCount"/> posts (Post N is N hours after BaseDate) and returns the options.</summary>
    private static async Task<DbContextOptions<Context>> SeedAsync(int postCount, int authorId = 1)
    {
        var options = TestDbContextFactory.CreateOptions();
        using var context = new Context(options);
        context.Profile.Add(TestDbContextFactory.Profile(1, "Jane", "Doe"));
        context.Profile.Add(TestDbContextFactory.Profile(2, "John", "Smith"));
        for (var i = 1; i <= postCount; i++)
        {
            context.Post.Add(TestDbContextFactory.Post(i, authorId, BaseDate.AddHours(i), $"Post {i}"));
        }
        await context.SaveChangesAsync();
        return options;
    }

    // A fresh context guarantees navigation properties are only populated by Include, not by change tracking.
    private static PostRepository NewRepository(DbContextOptions<Context> options) => new(new Context(options));

    [Fact]
    public async Task GetPostsByAuthorIdAsync_Returns_Only_Posts_For_Author_Newest_First()
    {
        using var context = TestDbContextFactory.Create();
        context.Post.AddRange(
            TestDbContextFactory.Post(1, 1, BaseDate, "A-old"),
            TestDbContextFactory.Post(2, 2, BaseDate.AddHours(1), "B"),
            TestDbContextFactory.Post(3, 1, BaseDate.AddHours(2), "A-new"));
        await context.SaveChangesAsync();

        var result = await new PostRepository(context).GetPostsByAuthorIdAsync(1);

        Assert.Equal(new[] { "A-new", "A-old" }, result.Select(p => p.Content));
    }

    [Fact]
    public async Task GetPostsByAuthorIdAsync_Returns_Empty_When_Author_Has_No_Posts()
    {
        var options = await SeedAsync(3, authorId: 1);

        Assert.Empty(await NewRepository(options).GetPostsByAuthorIdAsync(2));
    }

    [Fact]
    public async Task GetPostsPageAsync_Returns_First_Page_Newest_First_With_Total()
    {
        var options = await SeedAsync(15);

        var (posts, totalCount) = await NewRepository(options).GetPostsPageAsync(1, 10);

        Assert.Equal(15, totalCount);
        Assert.Equal(Enumerable.Range(6, 10).Reverse(), posts.Select(p => p.Id));
    }

    [Fact]
    public async Task GetPostsPageAsync_Skips_Earlier_Pages()
    {
        var options = await SeedAsync(15);

        var (posts, totalCount) = await NewRepository(options).GetPostsPageAsync(2, 10);

        Assert.Equal(15, totalCount);
        Assert.Equal(Enumerable.Range(1, 5).Reverse(), posts.Select(p => p.Id));
    }

    [Theory]
    [InlineData(1, 5, 5)]
    [InlineData(3, 5, 5)]
    [InlineData(4, 5, 0)]
    [InlineData(1, 100, 15)]
    [InlineData(8, 2, 1)]
    public async Task GetPostsPageAsync_Returns_Expected_Page_Size(int pageNumber, int pageSize, int expected)
    {
        var options = await SeedAsync(15);

        var (posts, _) = await NewRepository(options).GetPostsPageAsync(pageNumber, pageSize);

        Assert.Equal(expected, posts.Count);
    }

    [Fact]
    public async Task GetPostsPageAsync_Includes_Author()
    {
        var options = await SeedAsync(2);

        var (posts, _) = await NewRepository(options).GetPostsPageAsync(1, 10);

        Assert.All(posts, p => Assert.Equal("Jane Doe", p.Author?.FullName));
    }

    [Fact]
    public async Task GetPostsPageAsync_Returns_Empty_When_No_Posts()
    {
        var options = await SeedAsync(0);

        var (posts, totalCount) = await NewRepository(options).GetPostsPageAsync(1, 10);

        Assert.Empty(posts);
        Assert.Equal(0, totalCount);
    }

    [Fact]
    public async Task GetPostWithAuthorAsync_Returns_Post_With_Author()
    {
        var options = await SeedAsync(3);

        var result = await NewRepository(options).GetPostWithAuthorAsync(2);

        Assert.NotNull(result);
        Assert.Equal("Post 2", result!.Content);
        Assert.NotNull(result.Author);
        Assert.Equal("Jane Doe", result.Author.FullName);
    }

    [Fact]
    public async Task GetPostWithAuthorAsync_Returns_Null_When_Post_Not_Found()
    {
        var options = await SeedAsync(3);

        Assert.Null(await NewRepository(options).GetPostWithAuthorAsync(999));
    }
}
