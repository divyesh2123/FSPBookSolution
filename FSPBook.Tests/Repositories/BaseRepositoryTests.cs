using FSPBook.Data;
using FSPBook.Data.Entities;
using FSPBook.Repositroy.Implementations;
using FSPBook.Tests.Helpers;
using Xunit;

namespace FSPBook.Tests.Repositories;

public class BaseRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_Returns_Entity_When_It_Exists()
    {
        using var context = TestDbContextFactory.Create();
        context.Profile.Add(TestDbContextFactory.Profile(1, "Jane", "Doe"));
        await context.SaveChangesAsync();
        var repository = new BaseRepository<Profile>(context);

        var result = await repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Jane", result!.FirstName);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Entity_Does_Not_Exist()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new BaseRepository<Profile>(context);

        Assert.Null(await repository.GetByIdAsync(42));
    }

    [Fact]
    public async Task GetAllAsync_Returns_Empty_List_When_No_Entities()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new BaseRepository<Profile>(context);

        var result = await repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_Returns_All_Entities()
    {
        using var context = TestDbContextFactory.Create();
        context.Profile.AddRange(TestDbContextFactory.Profile(1), TestDbContextFactory.Profile(2), TestDbContextFactory.Profile(3));
        await context.SaveChangesAsync();
        var repository = new BaseRepository<Profile>(context);

        var result = await repository.GetAllAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task AddAsync_Does_Not_Persist_Until_SaveAsync_Is_Called()
    {
        var options = TestDbContextFactory.CreateOptions();
        using var context = new Context(options);
        var repository = new BaseRepository<Profile>(context);

        await repository.AddAsync(TestDbContextFactory.Profile(1));

        using (var verify = new Context(options))
        {
            Assert.Empty(verify.Profile);
        }

        await repository.SaveAsync();

        using (var verify = new Context(options))
        {
            Assert.Single(verify.Profile);
        }
    }

    [Fact]
    public async Task UpdateAsync_Persists_Changes_To_Tracked_Entity()
    {
        var options = TestDbContextFactory.CreateOptions();
        using (var context = new Context(options))
        {
            context.Profile.Add(TestDbContextFactory.Profile(1, job: "Developer"));
            await context.SaveChangesAsync();
        }

        using (var context = new Context(options))
        {
            var repository = new BaseRepository<Profile>(context);
            var profile = (await repository.GetByIdAsync(1))!;
            profile.JobTitle = "Architect";

            await repository.UpdateAsync(profile);
        }

        using (var context = new Context(options))
        {
            Assert.Equal("Architect", context.Profile.Single().JobTitle);
        }
    }

    [Fact]
    public async Task UpdateAsync_Persists_Changes_To_Detached_Entity()
    {
        var options = TestDbContextFactory.CreateOptions();
        using (var context = new Context(options))
        {
            context.Profile.Add(TestDbContextFactory.Profile(1, "Old", "Name"));
            await context.SaveChangesAsync();
        }

        using (var context = new Context(options))
        {
            await new BaseRepository<Profile>(context).UpdateAsync(TestDbContextFactory.Profile(1, "New", "Name"));
        }

        using (var context = new Context(options))
        {
            Assert.Equal("New", context.Profile.Single().FirstName);
        }
    }

    [Fact]
    public async Task DeleteAsync_Removes_Existing_Entity()
    {
        var options = TestDbContextFactory.CreateOptions();
        using (var context = new Context(options))
        {
            context.Profile.AddRange(TestDbContextFactory.Profile(1), TestDbContextFactory.Profile(2));
            await context.SaveChangesAsync();
        }

        using (var context = new Context(options))
        {
            await new BaseRepository<Profile>(context).DeleteAsync(1);
        }

        using (var context = new Context(options))
        {
            Assert.Equal(2, context.Profile.Single().Id);
        }
    }

    [Fact]
    public async Task DeleteAsync_Does_Nothing_When_Entity_Does_Not_Exist()
    {
        using var context = TestDbContextFactory.Create();
        context.Profile.Add(TestDbContextFactory.Profile(1));
        await context.SaveChangesAsync();
        var repository = new BaseRepository<Profile>(context);

        await repository.DeleteAsync(99);

        Assert.Single(context.Profile);
    }

    [Fact]
    public async Task SaveAsync_With_No_Changes_Does_Not_Throw()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new BaseRepository<Post>(context);

        var exception = await Record.ExceptionAsync(() => repository.SaveAsync());

        Assert.Null(exception);
    }
}
