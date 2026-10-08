using FSPBook.Data.Entities;
using FSPBook.Repositroy.Interfaces;
using FSPBook.Service.Interfaces;
using FSPBook.ViewModel;

namespace FSPBook.Service.Implementations;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IProfileRepository _profileRepository;

    public PostService(IPostRepository postRepository, IProfileRepository profileRepository)
    {
        _postRepository = postRepository;
        _profileRepository = profileRepository;
    }

    public async Task<PostViewModel?> GetPostByIdAsync(int id)
    {
        var post = await _postRepository.GetPostWithAuthorAsync(id);
        return post != null ? MapToViewModel(post) : null;
    }

    public async Task<List<PostViewModel>> GetAllPostsAsync()
    {
        var posts = await _postRepository.GetAllAsync();
        return posts.Select(MapToViewModel).ToList();
    }

    public async Task<List<PostViewModel>> GetPostsByAuthorIdAsync(int authorId)
    {
        var posts = await _postRepository.GetPostsByAuthorIdAsync(authorId);
        return posts.Select(MapToViewModel).ToList();
    }

    public async Task<List<PostViewModel>> GetRecentPostsAsync(int take = 10)
    {
        var posts = await _postRepository.GetPostsOrderedByDateDescendingAsync(take);
        return posts.Select(MapToViewModel).ToList();
    }

    public async Task CreatePostAsync(int authorId, string content)
    {
        var post = new Post
        {
            AuthorId = authorId,
            Content = content,
            DateTimePosted = DateTimeOffset.Now
        };
        await _postRepository.AddAsync(post);
        await _postRepository.SaveAsync();
    }

    public async Task DeletePostAsync(int id)
    {
        await _postRepository.DeleteAsync(id);
    }

    private PostViewModel MapToViewModel(Post post)
    {
        return new PostViewModel
        {
            Id = post.Id,
            Content = post.Content,
            DateTimePosted = post.DateTimePosted,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.FullName ?? "Unknown"
        };
    }
}
