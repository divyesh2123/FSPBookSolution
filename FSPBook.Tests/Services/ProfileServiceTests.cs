using FSPBook.Data.Entities;
using FSPBook.Repositroy.Interfaces;
using FSPBook.Service.Implementations;
using FSPBook.Tests.Helpers;
using Moq;
using Xunit;

namespace FSPBook.Tests.Services;

public class ProfileServiceTests
{
    private readonly Mock<IProfileRepository> _repository = new();
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        _service = new ProfileService(_repository.Object, TestMapper.Instance);
    }

    private static Profile Jane() => new() { Id = 1, FirstName = "Jane", LastName = "Doe", JobTitle = "Developer" };

    [Fact]
    public async Task GetProfileByIdAsync_Maps_Fields_With_Empty_Posts()
    {
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(Jane());

        var result = await _service.GetProfileByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("Jane Doe", result.FullName);
        Assert.Equal("Developer", result.JobTitle);
        Assert.Empty(result.Posts);
    }

    [Fact]
    public async Task GetProfileByIdAsync_Returns_Null_When_Not_Found()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Profile?)null);

        Assert.Null(await _service.GetProfileByIdAsync(99));
    }

    [Fact]
    public async Task GetAllProfilesAsync_Maps_All_Profiles()
    {
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Profile>
        {
            Jane(),
            new() { Id = 2, FirstName = "John", LastName = "Smith", JobTitle = "Consultant" }
        });

        var result = await _service.GetAllProfilesAsync();

        Assert.Equal(new[] { "Jane Doe", "John Smith" }, result.Select(p => p.FullName));
        Assert.All(result, p => Assert.Empty(p.Posts));
    }

    [Fact]
    public async Task GetAllProfilesAsync_Returns_Empty_List_When_No_Profiles()
    {
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Profile>());

        Assert.Empty(await _service.GetAllProfilesAsync());
    }

    [Fact]
    public async Task GetProfileWithPostsAsync_Returns_Null_When_Not_Found()
    {
        _repository.Setup(r => r.GetProfileWithPostsAsync(It.IsAny<int>())).ReturnsAsync((Profile?)null);

        Assert.Null(await _service.GetProfileWithPostsAsync(3));
    }

    [Fact]
    public async Task GetProfileWithPostsAsync_Orders_Posts_Newest_First_And_Sets_AuthorName()
    {
        var baseDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var profile = Jane();
        profile.Posts = new List<Post>
        {
            new() { Id = 1, AuthorId = 1, Content = "oldest", DateTimePosted = baseDate },
            new() { Id = 2, AuthorId = 1, Content = "newest", DateTimePosted = baseDate.AddDays(2) },
            new() { Id = 3, AuthorId = 1, Content = "middle", DateTimePosted = baseDate.AddDays(1) }
        };
        _repository.Setup(r => r.GetProfileWithPostsAsync(1)).ReturnsAsync(profile);

        var result = await _service.GetProfileWithPostsAsync(1);

        Assert.NotNull(result);
        Assert.Equal(new[] { "newest", "middle", "oldest" }, result!.Posts.Select(p => p.Content));
        Assert.Equal(new[] { 2, 3, 1 }, result.Posts.Select(p => p.Id));
        Assert.Equal(baseDate.AddDays(2), result.Posts[0].DateTimePosted);
        Assert.All(result.Posts, p =>
        {
            Assert.Equal("Jane Doe", p.AuthorName);
            Assert.Equal(1, p.AuthorId);
        });
    }

    [Fact]
    public async Task GetProfileWithPostsAsync_Returns_Empty_Posts_When_Profile_Has_None()
    {
        _repository.Setup(r => r.GetProfileWithPostsAsync(1)).ReturnsAsync(Jane());

        var result = await _service.GetProfileWithPostsAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Developer", result!.JobTitle);
        Assert.Empty(result.Posts);
    }
}
