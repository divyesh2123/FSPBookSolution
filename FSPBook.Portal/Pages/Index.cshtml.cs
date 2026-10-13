using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using FSPBook.OutboundService.Interfaces;
using FSPBook.Service.Interfaces;
using FSPBook.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace FSPBook.Pages;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IPostService _postService;
    private readonly INewsService _newsService;

    public IndexModel(ILogger<IndexModel> logger, IPostService postService, INewsService newsService)
    {
        _logger = logger;
        _postService = postService;
        _newsService = newsService;
    }

    public const int PageSize = 10;

    public IPagedList<PostViewModel> Posts { get; set; } = new StaticPagedList<PostViewModel>(new List<PostViewModel>(), 1, PageSize, 0);
    public List<NewsItemViewModel> News { get; set; } = new List<NewsItemViewModel>();

    // "page" is reserved by Razor Pages routing, so the query string uses "pageNumber".
    public async Task OnGetAsync(int? pageNumber)
    {
        Posts = await _postService.GetPostsPageAsync(pageNumber ?? 1, PageSize);
        News = await _newsService.GetTechnologyNewsAsync();
    }
}