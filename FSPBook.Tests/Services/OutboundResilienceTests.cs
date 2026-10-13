using System.Net;
using FSPBook.OutboundService.Extensions;
using FSPBook.OutboundService.Implementations;
using FSPBook.OutboundService.Interfaces;
using FSPBook.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FSPBook.Tests.Services;

public class OutboundResilienceTests
{
    private const string ValidJson = """{ "data": [ { "title": "Story", "url": "https://example.com", "source": "example.com" } ] }""";

    private static INewsService BuildNewsService(FakeHttpMessageHandler handler, int maxRetryAttempts = 3)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["NewsApiKey"] = "test-key",
                ["filterCategory"] = "tech",
                [NewsService.TopNewsUrlKey] = "https://api.thenewsapi.com/v1/news/top",
                ["OutboundServices:NewsApi:Resilience:MaxRetryAttempts"] = maxRetryAttempts.ToString(),
                ["OutboundServices:NewsApi:Resilience:RetryBaseDelay"] = "00:00:00"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddOutboundServices(configuration);
        services.AddHttpClient(OutboundServiceCollectionExtensions.NewsApiClientName)
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        return services.BuildServiceProvider().GetRequiredService<INewsService>();
    }

    private static FakeHttpMessageHandler FailThenSucceed(int failures, HttpStatusCode failureStatus)
    {
        var calls = 0;
        return new FakeHttpMessageHandler(_ => ++calls <= failures
            ? new HttpResponseMessage(failureStatus)
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ValidJson) });
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.RequestTimeout)]
    public async Task Retries_Transient_Failures_Then_Succeeds(HttpStatusCode status)
    {
        var handler = FailThenSucceed(2, status);

        var result = await BuildNewsService(handler).GetTechnologyNewsAsync();

        Assert.Single(result);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task Retries_When_Network_Throws()
    {
        var calls = 0;
        var handler = new FakeHttpMessageHandler(_ => ++calls == 1
            ? throw new HttpRequestException("network blip")
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ValidJson) });

        var result = await BuildNewsService(handler).GetTechnologyNewsAsync();

        Assert.Single(result);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Gives_Up_After_Max_Retries_And_Returns_Empty()
    {
        var handler = FakeHttpMessageHandler.Returning(HttpStatusCode.InternalServerError);

        var result = await BuildNewsService(handler, maxRetryAttempts: 2).GetTechnologyNewsAsync();

        Assert.Empty(result);
        Assert.Equal(3, handler.Requests.Count); // 1 attempt + 2 retries
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task Does_Not_Retry_Non_Transient_Failures(HttpStatusCode status)
    {
        var handler = FakeHttpMessageHandler.Returning(status);

        Assert.Empty(await BuildNewsService(handler).GetTechnologyNewsAsync());
        Assert.Single(handler.Requests);
    }
}
