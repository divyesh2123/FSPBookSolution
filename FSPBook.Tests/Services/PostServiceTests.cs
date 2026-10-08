using FSPBook.Data.Entities;
using FSPBook.Repositroy.Implementations;
using FSPBook.Repositroy.Interfaces;
using FSPBook.Service.Implementations;
using FSPBook.Tests.Helpers;
using Moq;
using Xunit;

namespace FSPBook.Tests.Services;

public class PostServiceTests
{
    private static readonly DateTimeOffset Posted = new(2024, 5, 1, 9, 30, 0, TimeSpan.Zero);

    private readonly Mock<IPostRepository> _postRepository = new();
    private readonly Mock<IProfileRepository> _profileRepository = new();
    private readonly PostService _service;

    public PostServiceTests()
    {
        _service = new PostService(_postRepository.Object, _profileRepository.Object);
    }

    private static Post PostWithAuthor(int id, int authorId = 1) => new()
    {
        Id = id,
        AuthorId = authorId,
        Content = $"Content {id}",
        DateTimePosted = Posted,
        Author = new Profile { Id = authorId, FirstName = "Jane", LastName = "Doe" }
    };

    [Fact]
    public async Task GetPostByIdAsync_Maps_All_Fields()
    {
        _postRepository.Setup(r => r.GetPostWithAuthorAsync(7)).ReturnsAsync(PostWithAuthor(7, authorId: 3));

        var result = await _service.GetPostByIdAsync(7);

        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
        Assert.Equal("Content 7", result.Content);
        Assert.Equal(Posted, result.DateTimePosted);
        Assert.Equal(3, result.AuthorId);
        Assert.Equal("Jane Doe", result.AuthorName);
    }

    [Fact]
    public async Task GetPostByIdAsync_Returns_Null_When_Not_Found()
    {
        _postRepository.Setup(r => r.GetPostWithAuthorAsync(It.IsAny<int>())).ReturnsAsync((Post?)null);

        Assert.Null(await _service.GetPostByIdAsync(1));
    }

    [Fact]
    public async Task GetPostByIdAsync_Uses_Unknown_When_Author_Missing()
    {
        var post = PostWithAuthor(1);
        post.Author = null!;
        _postRepository.Setup(r => r.GetPostWithAuthorAsync(1)).ReturnsAsync(post);

        var result = await _service.GetPostByIdAsync(1);

        Assert.Equal("Unknown", result!.AuthorName);
    }

    [Fact]
    public async Task GetAllPostsAsync_Maps_Every_Post()
    {
        _postRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Post> { PostWithAuthor(1), PostWithAuthor(2) });

        var result = await _service.GetAllPostsAsync();

        Assert.Equal(new[] { 1, 2 }, result.Select(p => p.Id));
    }

    [Fact]
    public async Task GetAllPostsAsync_Returns_Empty_List_When_No_Posts()
    {
        _postRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Post>());

        Assert.Empty(await _service.GetAllPostsAsync());
    }

    [Fact]
    public async Task GetPostsByAuthorIdAsync_Passes_AuthorId_And_Preserves_Order()
    {
        _postRepository.Setup(r => r.GetPostsByAuthorIdAsync(4))
            .ReturnsAsync(new List<Post> { PostWithAuthor(9, 4), PostWithAuthor(3, 4) });

        var result = await _service.GetPostsByAuthorIdAsync(4);

        Assert.Equal(new[] { 9, 3 }, result.Select(p => p.Id));
        _postRepository.Verify(r => r.GetPostsByAuthorIdAsync(4), Times.Once);
    }

    [Fact]
    public async Task GetRecentPostsAsync_Defaults_Take_To_Ten()
    {
        _postRepository.Setup(r => r.GetPostsOrderedByDateDescendingAsync(It.IsAny<int>())).ReturnsAsync(new List<Post>());

        await _service.GetRecentPostsAsync();

        _postRepository.Verify(r => r.GetPostsOrderedByDateDescendingAsync(10), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(20)]
    public async Task GetRecentPostsAsync_Passes_Take_Through(int take)
    {
        _postRepository.Setup(r => r.GetPostsOrderedByDateDescendingAsync(take))
            .ReturnsAsync(new List<Post> { PostWithAuthor(1) });

        var result = await _service.GetRecentPostsAsync(take);

        Assert.Single(result);
        _postRepository.Verify(r => r.GetPostsOrderedByDateDescendingAsync(take), Times.Once);
    }

    [Fact]
    public async Task CreatePostAsync_Adds_Post_Then_Saves()
    {
        Post? captured = null;
        var calls = new List<string>();
        _postRepository.Setup(r => r.AddAsync(It.IsAny<Post>()))
            .Callback<Post>(p => { captured = p; calls.Add("Add"); })
            .Returns(Task.CompletedTask);
        _postRepository.Setup(r => r.SaveAsync())
            .Callback(() => calls.Add("Save"))
            .Returns(Task.CompletedTask);
        var before = DateTimeOffset.Now;

        await _service.CreatePostAsync(5, "Hello world");

        Assert.Equal(new[] { "Add", "Save" }, calls);
        Assert.NotNull(captured);
        Assert.Equal(5, captured!.AuthorId);
        Assert.Equal("Hello world", captured.Content);
        Assert.Equal(0, captured.Id);
        Assert.InRange(captured.DateTimePosted, before, DateTimeOffset.Now);
    }

    [Fact]
    public async Task CreatePostAsync_Propagates_Repository_Failure()
    {
        _postRepository.Setup(r => r.AddAsync(It.IsAny<Post>())).Returns(Task.CompletedTask);
        _postRepository.Setup(r => r.SaveAsync()).ThrowsAsync(new InvalidOperationException("db down"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePostAsync(1, "x"));
    }

    [Fact]
    public async Task DeletePostAsync_Delegates_To_Repository()
    {
        await _service.DeletePostAsync(12);

        _postRepository.Verify(r => r.DeleteAsync(12), Times.Once);
    }

    [Fact]
    public async Task PostService_With_Real_Repositories_Creates_Reads_And_Deletes()
    {
        using var context = TestDbContextFactory.Create();
        context.Profile.Add(TestDbContextFactory.Profile(1, "Jane", "Doe"));
        await context.SaveChangesAsync();
        var service = new PostService(new PostRepository(context), new ProfileRepository(context));

        await service.CreatePostAsync(1, "First!");
        var post = Assert.Single(await service.GetRecentPostsAsync());

        Assert.Equal("First!", post.Content);
        Assert.Equal("Jane Doe", post.AuthorName);
        Assert.Equal("First!", (await service.GetPostByIdAsync(post.Id))!.Content);
        Assert.Single(await service.GetPostsByAuthorIdAsync(1));

        await service.DeletePostAsync(post.Id);

        Assert.Empty(await service.GetAllPostsAsync());
    }
}
