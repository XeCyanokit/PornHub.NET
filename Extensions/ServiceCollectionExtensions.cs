using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PornhubApiWrapper.Caching;

namespace PornhubApiWrapper.Extensions;

/// <summary>
/// Dependency injection helpers for registering the wrapper as a typed HttpClient.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers wrapper services, in-memory cache, and a typed HttpClient.
    /// </summary>
    public static IServiceCollection AddPornhubApiWrapper(this IServiceCollection services, Action<PornhubClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new PornhubClientOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(options);
        services.AddMemoryCache();
        services.TryAddSingleton<IPornhubCache, MemoryPornhubCache>();

        services.AddHttpClient("PornhubApiWrapper", httpClient =>
        {
            httpClient.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            httpClient.Timeout = options.Timeout;
            if (!string.IsNullOrWhiteSpace(options.UserAgent))
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            }
        });
        services.TryAddTransient<IPornhubClient>(sp => new PornhubClient(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("PornhubApiWrapper"),
            sp.GetRequiredService<PornhubClientOptions>(),
            sp.GetRequiredService<IPornhubCache>()));

        return services;
    }
}
