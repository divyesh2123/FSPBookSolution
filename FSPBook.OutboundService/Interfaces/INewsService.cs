using FSPBook.ViewModel;

namespace FSPBook.OutboundService.Interfaces;

public interface INewsService
{
    Task<List<NewsItemViewModel>> GetTechnologyNewsAsync(int limit = 5);
}
