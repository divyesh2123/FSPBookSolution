using AutoMapper;
using FSPBook.Data.Entities;
using FSPBook.Repositroy.Interfaces;
using FSPBook.Service.Interfaces;
using FSPBook.ViewModel;
using X.PagedList;

namespace FSPBook.Service.Implementations;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IMapper _mapper;

    public PostService(IPostRepository postRepository, IProfileRepository profileRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _profileRepository = profileRepository;
        _mapper = mapper;
    }

    public async Task<PostViewModel?> GetPostByIdAsync(int id)
    {
        var post = await _postRepository.GetPostWithAuthorAsync(id);
        return post != null ? _mapper.Map<PostViewModel>(post) : null;
    }

    public async Task<List<PostViewModel>> GetAllPostsAsync()
    {
        var posts = await _postRepository.GetAllAsync();
        return _mapper.Map<List<PostViewModel>>(posts);
    }

    public async Task<List<PostViewModel>> GetPostsByAuthorIdAsync(int authorId)
    {
        var posts = await _postRepository.GetPostsByAuthorIdAsync(authorId);
        return _mapper.Map<List<PostViewModel>>(posts);
    }

    public async Task<IPagedList<PostViewModel>> GetPostsPageAsync(int pageNumber, int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        var (posts, totalCount) = await _postRepository.GetPostsPageAsync(pageNumber, pageSize);
        return new StaticPagedList<PostViewModel>(_mapper.Map<List<PostViewModel>>(posts), pageNumber, pageSize, totalCount);
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
}
