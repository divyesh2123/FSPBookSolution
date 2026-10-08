using FSPBook.Pages;
using FSPBook.Service.Interfaces;
using FSPBook.ViewModel;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FSPBook.Tests.Pages;

public class IndexModelTests
{
    private readonly Mock<IPostService> _postService = new();
    private readonly Mock<INewsService> _newsService = new();
    private readonly IndexModel _model;

    public IndexModelTests()
    {
        _postService.Setup(s => s.GetRecentPostsAsync(It.IsAny<int>())).ReturnsAsync(new List<PostViewModel>());
        _newsService.Setup(s => s.GetTechnologyNewsAsync(It.IsAny<int>())).ReturnsAsync(new List<NewsItemViewModel>());
        _model = new IndexModel(NullLogger<IndexModel>.Instance, _postService.Object, _newsService.Object);
    }

    [Fact]
    public void Defaults_Are_Initialised()
    {
        Assert.Equal(10, _model.Count);
        Assert.Empty(_model.Posts);
        Assert.Empty(_model.News);
    }

    [Fact]
    public async Task OnGetAsync_Without_Count_Requests_Ten_Posts()
    {
        await _model.OnGetAsync(null);

        Assert.Equal(10, _model.Count);
        _postService.Verify(s => s.GetRecentPostsAsync(10), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task OnGetAsync_With_Count_Requests_That_Many_Posts(int count)
    {
        await _model.OnGetAsync(count);

        Assert.Equal(count, _model.Count);
        _postService.Verify(s => s.GetRecentPostsAsync(count), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_Populates_Posts_And_News()
    {
        var posts = new List<PostViewModel> { new() { Id = 1, Content = "Hi", AuthorName = "Jane Doe" } };
        var news = new List<NewsItemViewModel> { new() { Title = "Headline" } };
        _postService.Setup(s => s.GetRecentPostsAsync(10)).ReturnsAsync(posts);
        _newsService.Setup(s => s.GetTechnologyNewsAsync(It.IsAny<int>())).ReturnsAsync(news);

        await _model.OnGetAsync(null);

        Assert.Same(posts, _model.Posts);
        Assert.Same(news, _model.News);
    }

    [Fact]
    public async Task OnGetAsync_Requests_News_With_Default_Limit()
    {
        await _model.OnGetAsync(30);

        _newsService.Verify(s => s.GetTechnologyNewsAsync(5), Times.Once);
    }
}
