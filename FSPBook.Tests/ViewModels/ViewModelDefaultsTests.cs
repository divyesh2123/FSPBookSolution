using FSPBook.ViewModel;
using Xunit;

namespace FSPBook.Tests.ViewModels;

public class ViewModelDefaultsTests
{
    [Fact]
    public void PostViewModel_Strings_Default_To_Empty()
    {
        var vm = new PostViewModel();

        Assert.Equal(string.Empty, vm.Content);
        Assert.Equal(string.Empty, vm.AuthorName);
        Assert.Equal(0, vm.Id);
        Assert.Equal(0, vm.AuthorId);
        Assert.Equal(default, vm.DateTimePosted);
    }

    [Fact]
    public void ProfileViewModel_Strings_Default_To_Empty_And_Posts_To_Empty_List()
    {
        var vm = new ProfileViewModel();

        Assert.Equal(string.Empty, vm.FirstName);
        Assert.Equal(string.Empty, vm.LastName);
        Assert.Equal(string.Empty, vm.FullName);
        Assert.Equal(string.Empty, vm.JobTitle);
        Assert.NotNull(vm.Posts);
        Assert.Empty(vm.Posts);
    }

    [Fact]
    public void ProfileViewModel_Instances_Do_Not_Share_Posts_List()
    {
        var a = new ProfileViewModel();
        var b = new ProfileViewModel();

        a.Posts.Add(new PostViewModel());

        Assert.Empty(b.Posts);
    }

    [Fact]
    public void NewsItemViewModel_Defaults()
    {
        var vm = new NewsItemViewModel();

        Assert.Equal(string.Empty, vm.Title);
        Assert.Null(vm.Url);
        Assert.Null(vm.Source);
    }

    [Fact]
    public void CreatePostViewModel_Defaults()
    {
        var vm = new CreatePostViewModel();

        Assert.Equal(0, vm.ProfileId);
        Assert.Equal(string.Empty, vm.ContentInput);
    }
}
