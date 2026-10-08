using FSPBook.ViewModel;

namespace FSPBook.Service.Interfaces;

public interface INewsService
{
    Task<List<NewsItemViewModel>> GetTechnologyNewsAsync(int limit = 5);
}
