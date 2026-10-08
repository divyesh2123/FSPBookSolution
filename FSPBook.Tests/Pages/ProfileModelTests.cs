using FSPBook.Pages;
using FSPBook.Service.Interfaces;
using FSPBook.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Xunit;

namespace FSPBook.Tests.Pages;

public class ProfileModelTests
{
    private readonly Mock<IProfileService> _profileService = new();
    private readonly ProfileModel _model;

    public ProfileModelTests()
    {
        _model = new ProfileModel(_profileService.Object);
    }

    [Fact]
    public async Task OnGetAsync_Returns_NotFound_When_Id_Is_Zero()
    {
        var result = await _model.OnGetAsync(0);

        Assert.IsType<NotFoundResult>(result);
        _profileService.Verify(s => s.GetProfileWithPostsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task OnGetAsync_Returns_NotFound_When_No_Id_Supplied()
    {
        var result = await _model.OnGetAsync(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task OnGetAsync_Returns_NotFound_When_Profile_Does_Not_Exist()
    {
        _profileService.Setup(s => s.GetProfileWithPostsAsync(99)).ReturnsAsync((ProfileViewModel?)null);

        var result = await _model.OnGetAsync(99);

        Assert.IsType<NotFoundResult>(result);
        Assert.Null(_model.Profile);
    }

    [Fact]
    public async Task OnGetAsync_Returns_Page_With_Profile_When_Found()
    {
        var profile = new ProfileViewModel { Id = 3, FullName = "Maisie Jones" };
        _profileService.Setup(s => s.GetProfileWithPostsAsync(3)).ReturnsAsync(profile);

        var result = await _model.OnGetAsync(3);

        Assert.IsType<PageResult>(result);
        Assert.Equal(3, _model.Id);
        Assert.Same(profile, _model.Profile);
    }

    [Fact]
    public async Task OnGetAsync_Falls_Back_To_Bound_Id_When_Parameter_Is_Null()
    {
        var profile = new ProfileViewModel { Id = 4 };
        _profileService.Setup(s => s.GetProfileWithPostsAsync(4)).ReturnsAsync(profile);
        _model.Id = 4;

        var result = await _model.OnGetAsync(null);

        Assert.IsType<PageResult>(result);
        Assert.Same(profile, _model.Profile);
    }

    [Fact]
    public async Task OnGetAsync_Parameter_Overrides_Bound_Id()
    {
        _profileService.Setup(s => s.GetProfileWithPostsAsync(5)).ReturnsAsync(new ProfileViewModel { Id = 5 });
        _model.Id = 4;

        await _model.OnGetAsync(5);

        Assert.Equal(5, _model.Id);
        _profileService.Verify(s => s.GetProfileWithPostsAsync(5), Times.Once);
        _profileService.Verify(s => s.GetProfileWithPostsAsync(4), Times.Never);
    }
}
