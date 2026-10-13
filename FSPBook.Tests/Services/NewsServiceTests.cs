using System.Net;
using AutoMapper;
using FSPBook.OutboundService.Implementations;
using FSPBook.OutboundService.Mappings;
using FSPBook.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FSPBook.Tests.Services;

public class NewsServiceTests
{
    private const string ValidJson = """
        {
          "data": [
            { "title": "Story one", "url": "https://example.com/1", "source": "example.com" },
            { "title": "Story two", "url": "https://example.com/2", "source": "news.org" }
          ]
        }
        """;

    private readonly Mock<ILogger<NewsService>> _logger = new();

    private const string TopNewsUrl = "https://api.thenewsapi.com/v1/news/top";

    private static IConfiguration Config(string? apiKey = "test-key", string? category = "tech", string? topNewsUrl = TopNewsUrl) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NewsApiKey"] = apiKey,
                ["filterCategory"] = category,
                [NewsService.TopNewsUrlKey] = topNewsUrl
            })
            .Build();

    private static readonly IMapper Mapper =
        new MapperConfiguration(cfg => cfg.AddProfile<NewsApiProfile>()).CreateMapper();

    private NewsService CreateService(FakeHttpMessageHandler handler, IConfiguration? config = null) =>
        new(new HttpClient(handler), config ?? Config(), _logger.Object, Mapper);

    private void VerifyLogged(LogLevel level, Times times) =>
        _logger.Verify(l => l.Log(
            level,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Returns_Empty_And_Skips_Http_When_ApiKey_Missing(string? apiKey)
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, ValidJson);

        var result = await CreateService(handler, Config(apiKey)).GetTechnologyNewsAsync();

        Assert.Empty(result);
        Assert.Empty(handler.Requests);
        VerifyLogged(LogLevel.Warning, Times.Once());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Returns_Empty_And_Skips_Http_When_TopNewsUrl_Missing(string? topNewsUrl)
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, ValidJson);

        var result = await CreateService(handler, Config(topNewsUrl: topNewsUrl)).GetTechnologyNewsAsync();

        Assert.Empty(result);
        Assert.Empty(handler.Requests);
        VerifyLogged(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task Uses_TopNewsUrl_From_Configuration()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, ValidJson);

        await CreateService(handler, Config(topNewsUrl: "https://news.example.test/v2/top")).GetTechnologyNewsAsync();

        Assert.StartsWith("https://news.example.test/v2/top?api_token=", handler.Requests.Single().RequestUri!.ToString());
    }

    [Fact]
    public async Task Builds_Request_Url_From_Configuration_And_Default_Limit()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, ValidJson);

        await CreateService(handler, Config("abc123", "science")).GetTechnologyNewsAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(
            "https://api.thenewsapi.com/v1/news/top?api_token=abc123&categories=science&language=en&limit=5",
            request.RequestUri!.ToString());
    }

    [Fact]
    public async Task Passes_Custom_Limit_In_Request()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, ValidJson);

        await CreateService(handler).GetTechnologyNewsAsync(3);

        Assert.EndsWith("limit=3", handler.Requests.Single().RequestUri!.Query);
    }

    [Fact]
    public async Task Missing_Category_Sends_Empty_Category()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, ValidJson);

        await CreateService(handler, Config(category: null)).GetTechnologyNewsAsync();

        Assert.Contains("categories=&", handler.Requests.Single().RequestUri!.Query);
    }

    [Fact]
    public async Task Maps_News_Items_From_Response()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, ValidJson);

        var result = await CreateService(handler).GetTechnologyNewsAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Story one", result[0].Title);
        Assert.Equal("https://example.com/1", result[0].Url);
        Assert.Equal("example.com", result[0].Source);
        Assert.Equal("Story two", result[1].Title);
        Assert.Equal("https://example.com/2", result[1].Url);
        Assert.Equal("news.org", result[1].Source);
    }

    [Fact]
    public async Task Missing_Url_And_Source_Map_To_Null()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, """{ "data": [ { "title": "Only title" } ] }""");

        var item = Assert.Single(await CreateService(handler).GetTechnologyNewsAsync());

        Assert.Equal("Only title", item.Title);
        Assert.Null(item.Url);
        Assert.Null(item.Source);
    }

    [Fact]
    public async Task Null_Title_Maps_To_Empty_String()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, """{ "data": [ { "title": null, "url": null, "source": null } ] }""");

        var item = Assert.Single(await CreateService(handler).GetTechnologyNewsAsync());

        Assert.Equal(string.Empty, item.Title);
        Assert.Null(item.Url);
        Assert.Null(item.Source);
    }

    [Fact]
    public async Task Missing_Title_Maps_To_Empty_String()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, """{ "data": [ { "url": "https://example.com" } ] }""");

        var item = Assert.Single(await CreateService(handler).GetTechnologyNewsAsync());

        Assert.Equal(string.Empty, item.Title);
        Assert.Equal("https://example.com", item.Url);
    }

    [Fact]
    public async Task Ignores_Unknown_Fields_In_Response()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK,
            """{ "meta": { "found": 1 }, "data": [ { "uuid": "x", "title": "Story", "image_url": "i.png", "source": "a.com" } ] }""");

        var item = Assert.Single(await CreateService(handler).GetTechnologyNewsAsync());

        Assert.Equal("Story", item.Title);
        Assert.Equal("a.com", item.Source);
    }

    [Fact]
    public async Task Empty_Data_Array_Returns_Empty_List()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, """{ "data": [] }""");

        Assert.Empty(await CreateService(handler).GetTechnologyNewsAsync());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Returns_Empty_And_Logs_Warning_On_Non_Success_Status(HttpStatusCode status)
    {
        var handler = FakeHttpMessageHandler.Returning(status, ValidJson);

        Assert.Empty(await CreateService(handler).GetTechnologyNewsAsync());
        VerifyLogged(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task Returns_Empty_And_Logs_Warning_When_Data_Property_Missing()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, """{ "meta": { "found": 0 } }""");

        Assert.Empty(await CreateService(handler).GetTechnologyNewsAsync());
        VerifyLogged(LogLevel.Warning, Times.Once());
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("")]
    [InlineData("""{ "data": { "title": "object instead of array" } }""")]
    public async Task Returns_Empty_And_Logs_Error_On_Malformed_Response(string body)
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.OK, body);

        Assert.Empty(await CreateService(handler).GetTechnologyNewsAsync());
        VerifyLogged(LogLevel.Error, Times.Once());
    }

    [Fact]
    public async Task Returns_Empty_And_Logs_Error_When_Http_Call_Throws()
    {
        var handler = new FakeHttpMessageHandler(_ => throw new HttpRequestException("network down"));

        Assert.Empty(await CreateService(handler).GetTechnologyNewsAsync());
        VerifyLogged(LogLevel.Error, Times.Once());
    }
}
