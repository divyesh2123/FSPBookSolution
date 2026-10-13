using FSPBook.Pages;
using FSPBook.OutboundService.Interfaces;
using FSPBook.Service.Interfaces;
using FSPBook.ViewModel;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using X.PagedList;
using Xunit;

namespace FSPBook.Tests.Pages;

public class IndexModelTests
{
    private readonly Mock<IPostService> _postService = new();
    private readonly Mock<INewsService> _newsService = new();
    private readonly IndexModel _model;

    public IndexModelTests()
    {
        _postService.Setup(s => s.GetPostsPageAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(EmptyPage());
        _newsService.Setup(s => s.GetTechnologyNewsAsync(It.IsAny<int>())).ReturnsAsync(new List<NewsItemViewModel>());
        _model = new IndexModel(NullLogger<IndexModel>.Instance, _postService.Object, _newsService.Object);
    }

    private static IPagedList<PostViewModel> EmptyPage() => new StaticPagedList<PostViewModel>(new List<PostViewModel>(), 1, 10, 0);

    [Fact]
    public void Defaults_Are_Initialised()
    {
        Assert.Empty(_model.Posts);
        Assert.Equal(0, _model.Posts.TotalItemCount);
        Assert.Empty(_model.News);
    }

    [Fact]
    public async Task OnGetAsync_Without_PageNumber_Requests_First_Page()
    {
        await _model.OnGetAsync(null);

        _postService.Verify(s => s.GetPostsPageAsync(1, IndexModel.PageSize), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    public async Task OnGetAsync_With_PageNumber_Requests_That_Page(int pageNumber)
    {
        await _model.OnGetAsync(pageNumber);

        _postService.Verify(s => s.GetPostsPageAsync(pageNumber, IndexModel.PageSize), Times.Once);
    }

    [Fact]
    public async Task OnGetAsync_Populates_Posts_And_News()
    {
        var posts = new StaticPagedList<PostViewModel>(
            new List<PostViewModel> { new() { Id = 1, Content = "Hi", AuthorName = "Jane Doe" } }, 1, 10, 1);
        var news = new List<NewsItemViewModel> { new() { Title = "Headline" } };
        _postService.Setup(s => s.GetPostsPageAsync(1, IndexModel.PageSize)).ReturnsAsync(posts);
        _newsService.Setup(s => s.GetTechnologyNewsAsync(It.IsAny<int>())).ReturnsAsync(news);

        await _model.OnGetAsync(null);

        Assert.Same(posts, _model.Posts);
        Assert.Same(news, _model.News);
    }

    [Fact]
    public async Task OnGetAsync_Requests_News_With_Default_Limit()
    {
        await _model.OnGetAsync(3);

        _newsService.Verify(s => s.GetTechnologyNewsAsync(5), Times.Once);
    }
}
