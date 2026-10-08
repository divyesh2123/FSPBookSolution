using FSPBook.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FSPBook.Data;
public class Context : DbContext
{
    public virtual DbSet<Profile> Profile { get; set; }
    public virtual DbSet<Post> Post { get; set; }

    public Context(DbContextOptions<Context> options)
        : base(options)
    {
            
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
