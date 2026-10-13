using System.Net;
using FSPBook.OutboundService.Implementations;
using FSPBook.OutboundService.Interfaces;
using FSPBook.OutboundService.Mappings;
using FSPBook.OutboundService.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;

namespace FSPBook.OutboundService.Extensions;

public static class OutboundServiceCollectionExtensions
{
    public const string ConfigurationSection = "OutboundServices";
    public const string NewsApiClientName = "NewsApi";

    /// <summary>
    /// Registers all clients that call external (third-party) services, each wrapped in a
    /// Polly resilience pipeline: total timeout -> retry -> circuit breaker -> attempt timeout.
    /// </summary>
    public static IServiceCollection AddOutboundServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(NewsApiProfile));

        services.AddHttpClient<INewsService, NewsService>(NewsApiClientName)
            .AddOutboundResilience(NewsApiClientName, configuration);

        return services;
    }

    private static IHttpClientBuilder AddOutboundResilience(this IHttpClientBuilder builder, string clientName, IConfiguration configuration)
    {
        var settings = new ResilienceSettings();
        configuration.GetSection($"{ConfigurationSection}:{clientName}:Resilience").Bind(settings);

        builder.AddResilienceHandler($"{clientName}-pipeline", (pipeline, context) =>
        {
            var logger = context.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger($"FSPBook.OutboundService.{clientName}");

            pipeline
                .AddTimeout(settings.TotalRequestTimeout)
                .AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = settings.MaxRetryAttempts,
                    Delay = settings.RetryBaseDelay,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    // Default ShouldHandle covers HttpRequestException, attempt timeouts, 408, 429 and 5xx.
                    OnRetry = args =>
                    {
                        logger.LogWarning(
                            args.Outcome.Exception,
                            "Retrying {Client} call (attempt {Attempt}) after {Delay}ms. Status: {StatusCode}",
                            clientName,
                            args.AttemptNumber + 1,
                            args.RetryDelay.TotalMilliseconds,
                            args.Outcome.Result?.StatusCode);
                        return default;
                    }
                })
                .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = settings.CircuitBreakerFailureRatio,
                    MinimumThroughput = settings.CircuitBreakerMinimumThroughput,
                    SamplingDuration = settings.CircuitBreakerSamplingDuration,
                    BreakDuration = settings.CircuitBreakerBreakDuration,
                    OnOpened = args =>
                    {
                        logger.LogError("Circuit opened for {Client} for {BreakDuration}", clientName, args.BreakDuration);
                        return default;
                    },
                    OnClosed = _ =>
                    {
                        logger.LogInformation("Circuit closed for {Client}", clientName);
                        return default;
                    }
                })
                .AddTimeout(settings.AttemptTimeout);
        });

        return builder;
    }
}
