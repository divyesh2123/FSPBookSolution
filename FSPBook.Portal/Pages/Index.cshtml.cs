using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using FSPBook.Service.Interfaces;
using FSPBook.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public List<PostViewModel> Posts { get; set; } = new List<PostViewModel>();
    public int Count { get; set; } = 10;
    public List<NewsItemViewModel> News { get; set; } = new List<NewsItemViewModel>();

    public async Task OnGetAsync(int? count)
    {
        Count = count ?? 10;
        Posts = await _postService.GetRecentPostsAsync(Count);
        News = await _newsService.GetTechnologyNewsAsync();
    }
}