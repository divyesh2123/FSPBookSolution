using System.ComponentModel.DataAnnotations;
using FSPBook.Data;
using FSPBook.Pages;
using FSPBook.Service.Interfaces;
using FSPBook.Tests.Helpers;
using Moq;
using Xunit;

namespace FSPBook.Tests.Pages;

public class CreateModelTests : IDisposable
{
    private readonly Context _context;
    private readonly Mock<IProfileService> _profileService = new();
    private readonly Mock<IPostService> _postService = new();
    private readonly CreateModel _model;

    public CreateModelTests()
    {
        _context = TestDbContextFactory.Create();
        _context.Profile.AddRange(
            TestDbContextFactory.Profile(1, "Jane", "Doe"),
            TestDbContextFactory.Profile(2, "John", "Smith"));
        _context.SaveChanges();
        _model = new CreateModel(_context, _profileService.Object, _postService.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task OnGetAsync_Loads_All_Profiles()
    {
        await _model.OnGetAsync();

        Assert.Equal(new[] { "Jane Doe", "John Smith" }, _model.Profiles.Select(p => p.FullName).OrderBy(n => n));
        Assert.False(_model.Success);
    }

    [Fact]
    public async Task OnGetAsync_Loads_Empty_List_When_No_Profiles()
    {
        using var empty = TestDbContextFactory.Create();
        var model = new CreateModel(empty, _profileService.Object, _postService.Object);

        await model.OnGetAsync();

        Assert.NotNull(model.Profiles);
        Assert.Empty(model.Profiles);
    }

    [Fact]
    public async Task OnPostAsync_Creates_Post_And_Resets_Form_For_Selected_Profile()
    {
        _model.ProfileId = 2;
        _model.ContentInput = "Hello!";

        await _model.OnPostAsync();

        _postService.Verify(s => s.CreatePostAsync(2, "Hello!"), Times.Once);
        Assert.True(_model.Success);
        Assert.Equal(string.Empty, _model.ContentInput);
        Assert.Equal(2, _model.Profiles.Count);
    }

    [Fact]
    public async Task OnPostAsync_Does_Not_Create_Post_When_No_Profile_Selected()
    {
        _model.ProfileId = -1;
        _model.ContentInput = "Hello!";

        await _model.OnPostAsync();

        _postService.Verify(s => s.CreatePostAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        Assert.False(_model.Success);
        Assert.Equal("Hello!", _model.ContentInput);
        Assert.Equal(2, _model.Profiles.Count);
    }

    [Fact]
    public async Task OnPostAsync_Propagates_Service_Failure()
    {
        _model.ProfileId = 1;
        _model.ContentInput = "x";
        _postService.Setup(s => s.CreatePostAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _model.OnPostAsync());
        Assert.False(_model.Success);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(10000, true)]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(10001, false)]
    public void ProfileId_Validation_Requires_Value_In_Range(int profileId, bool expectedValid)
    {
        _model.ProfileId = profileId;

        var results = Validate(nameof(CreateModel.ProfileId), _model.ProfileId);

        Assert.Equal(expectedValid, results.Count == 0);
        if (!expectedValid)
        {
            Assert.Equal("Choose a person to post on behalf of", results.Single().ErrorMessage);
        }
    }

    [Theory]
    [InlineData(null, "Write a post")]
    [InlineData("", "Write a post")]
    public void ContentInput_Validation_Rejects_Missing_Content(string? content, string expectedMessage)
    {
        var results = Validate(nameof(CreateModel.ContentInput), content);

        Assert.Contains(results, r => r.ErrorMessage == expectedMessage);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("A longer post with content")]
    public void ContentInput_Validation_Accepts_Content(string content)
    {
        Assert.Empty(Validate(nameof(CreateModel.ContentInput), content));
    }

    private List<ValidationResult> Validate(string propertyName, object? value)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(_model) { MemberName = propertyName };
        Validator.TryValidateProperty(value, context, results);
        return results;
    }
}
