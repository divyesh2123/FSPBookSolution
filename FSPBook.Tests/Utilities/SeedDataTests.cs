using FSPBook.Data;
using FSPBook.Tests.Helpers;
using FSPBook.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FSPBook.Tests.Utilities;

public class SeedDataTests
{
    private static ServiceProvider BuildProvider(string databaseName) =>
        new ServiceCollection()
            .AddDbContext<Context>(o => o.UseInMemoryDatabase(databaseName))
            .BuildServiceProvider();

    private static Context OpenContext(IServiceProvider provider) =>
        new(provider.GetRequiredService<DbContextOptions<Context>>());

    [Fact]
    public void Seed_Populates_Empty_Database_With_Profiles_And_Posts()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString());

        SeedData.Seed(provider);

        using var context = OpenContext(provider);
        Assert.Equal(7, context.Profile.Count());
        Assert.Equal(2, context.Post.Count());
        Assert.Equal("Jane Doe", context.Profile.Single(p => p.Id == 1).FullName);
        Assert.Equal("Sinead O'Leary", context.Profile.Single(p => p.Id == 7).FullName);
    }

    [Fact]
    public void Seed_Creates_Posts_Linked_To_Existing_Profiles()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString());

        SeedData.Seed(provider);

        using var context = OpenContext(provider);
        var posts = context.Post.Include(p => p.Author).OrderBy(p => p.Id).ToList();
        Assert.Equal(new[] { 1, 2 }, posts.Select(p => p.AuthorId));
        Assert.All(posts, p =>
        {
            Assert.NotNull(p.Author);
            Assert.StartsWith("Lorem Ipsum", p.Content);
        });
        Assert.True(posts[1].DateTimePosted > posts[0].DateTimePosted);
    }

    [Fact]
    public void Seed_Is_Idempotent()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString());

        SeedData.Seed(provider);
        SeedData.Seed(provider);

        using var context = OpenContext(provider);
        Assert.Equal(7, context.Profile.Count());
        Assert.Equal(2, context.Post.Count());
    }

    [Fact]
    public void Seed_Skips_When_Profiles_Already_Exist()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString());
        using (var context = OpenContext(provider))
        {
            context.Profile.Add(TestDbContextFactory.Profile(100, "Existing", "User"));
            context.SaveChanges();
        }

        SeedData.Seed(provider);

        using (var context = OpenContext(provider))
        {
            Assert.Equal("Existing User", context.Profile.Single().FullName);
            Assert.Empty(context.Post);
        }
    }

    [Fact]
    public void Seed_Throws_When_DbContextOptions_Not_Registered()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => SeedData.Seed(provider));
    }
}
