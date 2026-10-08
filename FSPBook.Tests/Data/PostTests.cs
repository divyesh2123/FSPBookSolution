using System;
using System.Linq;
using System.Threading.Tasks;
using FSPBook.Data;
using FSPBook.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FSPBook.Tests;
public class PostTests
{
    [Fact]
    public async Task Posts_Are_Returned_In_Descending_Order()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Seed
        using (var context = new Context(options))
        {
            context.Profile.Add(new Profile { Id = 1, FirstName = "A", LastName = "B" });
            context.Post.Add(new Post { Id = 1, AuthorId = 1, Content = "Old", DateTimePosted = DateTimeOffset.UtcNow.AddDays(-1) });
            context.Post.Add(new Post { Id = 2, AuthorId = 1, Content = "New", DateTimePosted = DateTimeOffset.UtcNow });
            await context.SaveChangesAsync();
        }

        using (var context = new Context(options))
        {
            var posts = await context.Post.OrderByDescending(p => p.DateTimePosted).ToListAsync();
            Assert.Equal(2, posts.Count);
            Assert.Equal("New", posts.First().Content);
        }
    }
}
