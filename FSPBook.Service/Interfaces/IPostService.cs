using FSPBook.ViewModel;
using X.PagedList;

namespace FSPBook.Service.Interfaces;

public interface IPostService
{
    Task<PostViewModel?> GetPostByIdAsync(int id);
    Task<List<PostViewModel>> GetAllPostsAsync();
    Task<List<PostViewModel>> GetPostsByAuthorIdAsync(int authorId);
    Task<IPagedList<PostViewModel>> GetPostsPageAsync(int pageNumber, int pageSize = 10);
    Task CreatePostAsync(int authorId, string content);
    Task DeletePostAsync(int id);
}
