using FSPBook.Data.Entities;

namespace FSPBook.Repositroy.Interfaces;

public interface IPostRepository : IBaseRepository<Post>
{
    Task<List<Post>> GetPostsByAuthorIdAsync(int authorId);
    Task<List<Post>> GetPostsOrderedByDateDescendingAsync(int take = 10);
    Task<Post?> GetPostWithAuthorAsync(int postId);
}
