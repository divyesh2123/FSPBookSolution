using FSPBook.ViewModel;

namespace FSPBook.Service.Interfaces;

public interface IPostService
{
    Task<PostViewModel?> GetPostByIdAsync(int id);
    Task<List<PostViewModel>> GetAllPostsAsync();
    Task<List<PostViewModel>> GetPostsByAuthorIdAsync(int authorId);
    Task<List<PostViewModel>> GetRecentPostsAsync(int take = 10);
    Task CreatePostAsync(int authorId, string content);
    Task DeletePostAsync(int id);
}
