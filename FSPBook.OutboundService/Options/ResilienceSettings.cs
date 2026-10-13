namespace FSPBook.OutboundService.Options;

/// <summary>
/// Polly resilience settings applied to an outbound HTTP client.
/// Bound from "OutboundServices:{ClientName}:Resilience" in configuration.
/// </summary>
public class ResilienceSettings
{
    /// <summary>Number of retries after the first failed attempt.</summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Base delay for exponential backoff (with jitter) between retries.</summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Timeout applied to each individual attempt.</summary>
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Timeout for the whole call, including all retries.</summary>
    public TimeSpan TotalRequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Failure ratio (0-1) within the sampling window that opens the circuit.</summary>
    public double CircuitBreakerFailureRatio { get; set; } = 0.5;

    /// <summary>Minimum number of calls in the sampling window before the circuit can open.</summary>
    public int CircuitBreakerMinimumThroughput { get; set; } = 10;

    /// <summary>Window over which failures are sampled.</summary>
    public TimeSpan CircuitBreakerSamplingDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>How long the circuit stays open before allowing a trial call.</summary>
    public TimeSpan CircuitBreakerBreakDuration { get; set; } = TimeSpan.FromSeconds(30);
}
