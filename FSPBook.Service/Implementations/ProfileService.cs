using FSPBook.Repositroy.Interfaces;
using FSPBook.Service.Interfaces;
using FSPBook.ViewModel;

namespace FSPBook.Service.Implementations;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;

    public ProfileService(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<ProfileViewModel?> GetProfileByIdAsync(int id)
    {
        var profile = await _profileRepository.GetByIdAsync(id);
        return profile != null ? MapToViewModel(profile, new List<PostViewModel>()) : null;
    }

    public async Task<List<ProfileViewModel>> GetAllProfilesAsync()
    {
        var profiles = await _profileRepository.GetAllAsync();
        return profiles.Select(p => MapToViewModel(p, new List<PostViewModel>())).ToList();
    }

    public async Task<ProfileViewModel?> GetProfileWithPostsAsync(int id)
    {
        var profile = await _profileRepository.GetProfileWithPostsAsync(id);
        if (profile == null) return null;

        var posts = profile.Posts
            .OrderByDescending(p => p.DateTimePosted)
            .Select(p => new PostViewModel
            {
                Id = p.Id,
                Content = p.Content,
                DateTimePosted = p.DateTimePosted,
                AuthorId = p.AuthorId,
                AuthorName = profile.FullName
            })
            .ToList();

        return MapToViewModel(profile, posts);
    }

    private ProfileViewModel MapToViewModel(FSPBook.Data.Entities.Profile profile, List<PostViewModel> posts)
    {
        return new ProfileViewModel
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            FullName = profile.FullName,
            JobTitle = profile.JobTitle,
            Posts = posts
        };
    }
}
