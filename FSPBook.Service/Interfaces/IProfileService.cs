using FSPBook.ViewModel;

namespace FSPBook.Service.Interfaces;

public interface IProfileService
{
    Task<ProfileViewModel?> GetProfileByIdAsync(int id);
    Task<List<ProfileViewModel>> GetAllProfilesAsync();
    Task<ProfileViewModel?> GetProfileWithPostsAsync(int id);
}
