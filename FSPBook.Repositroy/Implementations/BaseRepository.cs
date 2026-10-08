using FSPBook.Data;
using FSPBook.Repositroy.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FSPBook.Repositroy.Implementations;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly Context _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(Context context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await SaveAsync();
        }
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
