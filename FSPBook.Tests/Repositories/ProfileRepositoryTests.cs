using FSPBook.Data;
using FSPBook.Repositroy.Implementations;
using FSPBook.Tests.Helpers;
using Xunit;

namespace FSPBook.Tests.Repositories;

public class ProfileRepositoryTests
{
    [Fact]
    public async Task GetProfileWithPostsAsync_Returns_Profile_With_Only_Its_Posts()
    {
        var options = TestDbContextFactory.CreateOptions();
        using (var context = new Context(options))
        {
            context.Profile.AddRange(TestDbContextFactory.Profile(1), TestDbContextFactory.Profile(2));
            context.Post.AddRange(
                TestDbContextFactory.Post(1, 1, DateTimeOffset.UtcNow),
                TestDbContextFactory.Post(2, 1, DateTimeOffset.UtcNow),
                TestDbContextFactory.Post(3, 2, DateTimeOffset.UtcNow));
            await context.SaveChangesAsync();
        }

        using (var context = new Context(options))
        {
            var result = await new ProfileRepository(context).GetProfileWithPostsAsync(1);

            Assert.NotNull(result);
            Assert.Equal(new[] { 1, 2 }, result!.Posts.Select(p => p.Id).OrderBy(id => id));
        }
    }

    [Fact]
    public async Task GetProfileWithPostsAsync_Returns_Empty_Posts_When_Profile_Has_None()
    {
        using var context = TestDbContextFactory.Create();
        context.Profile.Add(TestDbContextFactory.Profile(1));
        await context.SaveChangesAsync();

        var result = await new ProfileRepository(context).GetProfileWithPostsAsync(1);

        Assert.NotNull(result);
        Assert.Empty(result!.Posts);
    }

    [Fact]
    public async Task GetProfileWithPostsAsync_Returns_Null_When_Profile_Not_Found()
    {
        using var context = TestDbContextFactory.Create();

        Assert.Null(await new ProfileRepository(context).GetProfileWithPostsAsync(5));
    }

    [Fact]
    public async Task Inherited_Base_Methods_Work_For_Profiles()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ProfileRepository(context);

        await repository.AddAsync(TestDbContextFactory.Profile(1, "Jane", "Doe"));
        await repository.SaveAsync();

        Assert.Single(await repository.GetAllAsync());
        Assert.Equal("Jane Doe", (await repository.GetByIdAsync(1))!.FullName);
    }
}
