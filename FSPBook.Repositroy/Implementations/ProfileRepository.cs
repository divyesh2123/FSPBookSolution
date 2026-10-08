using FSPBook.Data;
using FSPBook.Data.Entities;
using FSPBook.Repositroy.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FSPBook.Repositroy.Implementations;

public class ProfileRepository : BaseRepository<Profile>, IProfileRepository
{
    public ProfileRepository(Context context) : base(context)
    {
    }

    public async Task<Profile?> GetProfileWithPostsAsync(int profileId)
    {
        return await _dbSet
            .Include(p => p.Posts)
            .FirstOrDefaultAsync(p => p.Id == profileId);
    }
}
