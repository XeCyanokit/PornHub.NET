namespace PornhubApiWrapper;

/// <summary>
/// Configuration for transport behavior, resiliency, pagination, and caching.
/// </summary>
public sealed class PornhubClientOptions
{
    /// <summary>Base URL for webmasters endpoints.</summary>
    public string BaseUrl { get; set; } = "https://www.pornhub.com/webmasters/";
    /// <summary>Throw exceptions on non-success status codes.</summary>
    public bool ThrowOnHttpErrors { get; set; } = true;
    /// <summary>User-Agent header sent by the wrapper.</summary>
    public string UserAgent { get; set; } = "PornhubApiWrapper/1.0";
    /// <summary>Request timeout used by the client.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    /// <summary>Enable endpoint-specific compatibility fallbacks.</summary>
    public bool EnableCompatibilityFallbacks { get; set; } = true;
    /// <summary>Pagination defaults and limits.</summary>
    public PaginationDefaults Pagination { get; set; } = new();
    /// <summary>Resilience policy options.</summary>
    public ResilienceOptions Resilience { get; set; } = new();
    /// <summary>Caching policy options.</summary>
    public CacheOptions Cache { get; set; } = new();
}

/// <summary>
/// Default and maximum pagination values.
/// </summary>
public sealed class PaginationDefaults
{
    public int DefaultPage { get; set; } = 1;
    public int DefaultPerPage { get; set; } = 30;
    public int MaxPerPage { get; set; } = 100;
}

/// <summary>
/// Retry and circuit breaker behavior.
/// </summary>
public sealed class ResilienceOptions
{
    public int MaxRetries { get; set; } = 2;
    public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromMilliseconds(300);
    public bool RetryOn429 { get; set; } = true;
    public bool RetryOn5xx { get; set; } = true;
    public bool RetryOn408 { get; set; } = true;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(20);
}

/// <summary>
/// Cache behavior for endpoint payloads.
/// </summary>
public sealed class CacheOptions
{
    public bool Enabled { get; set; } = false;
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromMinutes(5);
    public bool StaleWhileRevalidate { get; set; } = false;
    public Dictionary<string, TimeSpan> EndpointTtls { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["categories"] = TimeSpan.FromHours(12),
        ["tags"] = TimeSpan.FromHours(6),
        ["stars"] = TimeSpan.FromMinutes(30),
        ["search"] = TimeSpan.FromMinutes(2),
        ["video_by_id"] = TimeSpan.FromMinutes(10),
        ["is_video_active"] = TimeSpan.FromMinutes(1),
        ["deleted_videos"] = TimeSpan.FromMinutes(2)
    };
}
