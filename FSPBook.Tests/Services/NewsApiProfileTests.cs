using AutoMapper;
using FSPBook.OutboundService.Mappings;
using FSPBook.OutboundService.Models;
using FSPBook.ViewModel;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FSPBook.Tests.Services;

public class NewsApiProfileTests
{
    private readonly MapperConfiguration _configuration =
        new(cfg => cfg.AddProfile<NewsApiProfile>(), NullLoggerFactory.Instance);

    [Fact]
    public void Configuration_Is_Valid()
    {
        _configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Maps_All_Fields()
    {
        var article = new NewsApiArticle { Title = "Story", Url = "https://example.com", Source = "example.com" };

        var result = _configuration.CreateMapper().Map<NewsItemViewModel>(article);

        Assert.Equal("Story", result.Title);
        Assert.Equal("https://example.com", result.Url);
        Assert.Equal("example.com", result.Source);
    }

    [Fact]
    public void Null_Title_Maps_To_Empty_String()
    {
        var result = _configuration.CreateMapper().Map<NewsItemViewModel>(new NewsApiArticle());

        Assert.Equal(string.Empty, result.Title);
        Assert.Null(result.Url);
        Assert.Null(result.Source);
    }
}
