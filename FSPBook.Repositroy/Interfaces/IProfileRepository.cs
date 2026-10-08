using FSPBook.Data.Entities;

namespace FSPBook.Repositroy.Interfaces;

public interface IProfileRepository : IBaseRepository<Profile>
{
    Task<Profile?> GetProfileWithPostsAsync(int profileId);
}
