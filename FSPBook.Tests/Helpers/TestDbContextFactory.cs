using FSPBook.Data;
using FSPBook.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FSPBook.Tests.Helpers;

public static class TestDbContextFactory
{
    public static DbContextOptions<Context> CreateOptions() =>
        new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    public static Context Create() => new Context(CreateOptions());

    public static Profile Profile(int id, string first = "First", string last = "Last", string job = "Developer") =>
        new Profile { Id = id, FirstName = first, LastName = last, JobTitle = job };

    public static Post Post(int id, int authorId, DateTimeOffset postedAt, string content = "Content") =>
        new Post { Id = id, AuthorId = authorId, Content = content, DateTimePosted = postedAt };
}
