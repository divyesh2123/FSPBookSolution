using FSPBook.Data;
using FSPBook.Data.Entities;
using FSPBook.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FSPBook.Tests.Data;

public class EntityTests
{
    [Theory]
    [InlineData("Jane", "Doe", "Jane Doe")]
    [InlineData("Jane", "", "Jane ")]
    [InlineData("", "Doe", " Doe")]
    [InlineData(null, null, " ")]
    public void Profile_FullName_Concatenates_First_And_Last_Name(string? first, string? last, string expected)
    {
        var profile = new Profile { FirstName = first!, LastName = last! };

        Assert.Equal(expected, profile.FullName);
    }

    [Fact]
    public void Profile_Posts_Defaults_To_Empty_Collection()
    {
        var profile = new Profile();

        Assert.NotNull(profile.Posts);
        Assert.Empty(profile.Posts);
    }

    [Fact]
    public void Post_Defaults_Are_Unset()
    {
        var post = new Post();

        Assert.Equal(0, post.Id);
        Assert.Equal(0, post.AuthorId);
        Assert.Null(post.Content);
        Assert.Null(post.Author);
        Assert.Equal(default, post.DateTimePosted);
    }

    [Fact]
    public async Task Context_Persists_Profile_And_Post_With_Relationship()
    {
        var options = TestDbContextFactory.CreateOptions();
        using (var context = new Context(options))
        {
            context.Profile.Add(TestDbContextFactory.Profile(1, "Jane", "Doe"));
            context.Post.Add(TestDbContextFactory.Post(1, 1, DateTimeOffset.UtcNow));
            await context.SaveChangesAsync();
        }

        using (var context = new Context(options))
        {
            var post = await context.Post.Include(p => p.Author).SingleAsync();
            var profile = await context.Profile.Include(p => p.Posts).SingleAsync();

            Assert.Equal("Jane Doe", post.Author.FullName);
            Assert.Single(profile.Posts);
            Assert.Equal(1, profile.Posts.First().Id);
        }
    }

    [Fact]
    public void Context_Does_Not_Map_FullName()
    {
        using var context = TestDbContextFactory.Create();
        var entityType = context.Model.FindEntityType(typeof(Profile))!;

        Assert.Null(entityType.FindProperty(nameof(Profile.FullName)));
        Assert.NotNull(entityType.FindProperty(nameof(Profile.FirstName)));
    }
}
