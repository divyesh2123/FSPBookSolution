namespace FSPBook.OutboundService.Options;

/// <summary>
/// Polly retry settings applied to an outbound HTTP client.
/// Bound from "OutboundServices:{ClientName}:Resilience" in configuration.
/// </summary>
public class ResilienceSettings
{
    /// <summary>Number of retries after the first failed attempt.</summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Base delay for exponential backoff (with jitter) between retries.</summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);
}
