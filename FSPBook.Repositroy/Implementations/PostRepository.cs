using FSPBook.Data;
using FSPBook.Data.Entities;
using FSPBook.Repositroy.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FSPBook.Repositroy.Implementations;

public class PostRepository : BaseRepository<Post>, IPostRepository
{
    public PostRepository(Context context) : base(context)
    {
    }

    public async Task<List<Post>> GetPostsByAuthorIdAsync(int authorId)
    {
        return await _dbSet
            .Where(p => p.AuthorId == authorId)
            .OrderByDescending(p => p.DateTimePosted)
            .ToListAsync();
    }

    public async Task<(List<Post> Posts, int TotalCount)> GetPostsPageAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _dbSet.CountAsync();

        var posts = await _dbSet
            .Include(p => p.Author)
            .OrderByDescending(p => p.DateTimePosted)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task<Post?> GetPostWithAuthorAsync(int postId)
    {
        return await _dbSet
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }
}
