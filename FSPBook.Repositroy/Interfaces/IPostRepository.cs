using FSPBook.Data.Entities;

namespace FSPBook.Repositroy.Interfaces;

public interface IPostRepository : IBaseRepository<Post>
{
    Task<List<Post>> GetPostsByAuthorIdAsync(int authorId);
    /// <summary>Returns one page of posts (newest first, with author) and the total number of posts.</summary>
    Task<(List<Post> Posts, int TotalCount)> GetPostsPageAsync(int pageNumber, int pageSize);
    Task<Post?> GetPostWithAuthorAsync(int postId);
}
