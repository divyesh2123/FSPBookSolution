using AutoMapper;
using FSPBook.Repositroy.Interfaces;
using FSPBook.Service.Interfaces;
using FSPBook.ViewModel;

namespace FSPBook.Service.Implementations;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IMapper _mapper;

    public ProfileService(IProfileRepository profileRepository, IMapper mapper)
    {
        _profileRepository = profileRepository;
        _mapper = mapper;
    }

    public async Task<ProfileViewModel?> GetProfileByIdAsync(int id)
    {
        var profile = await _profileRepository.GetByIdAsync(id);
        return profile != null ? _mapper.Map<ProfileViewModel>(profile) : null;
    }

    public async Task<List<ProfileViewModel>> GetAllProfilesAsync()
    {
        var profiles = await _profileRepository.GetAllAsync();
        return _mapper.Map<List<ProfileViewModel>>(profiles);
    }

    public async Task<ProfileViewModel?> GetProfileWithPostsAsync(int id)
    {
        var profile = await _profileRepository.GetProfileWithPostsAsync(id);
        if (profile == null) return null;

        var viewModel = _mapper.Map<ProfileViewModel>(profile);
        viewModel.Posts = _mapper.Map<List<PostViewModel>>(profile.Posts.OrderByDescending(p => p.DateTimePosted));
        viewModel.Posts.ForEach(p => p.AuthorName = profile.FullName);

        return viewModel;
    }
}
