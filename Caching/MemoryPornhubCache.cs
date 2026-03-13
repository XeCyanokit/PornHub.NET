using Microsoft.Extensions.Caching.Memory;

namespace PornhubApiWrapper.Caching;

public sealed class MemoryPornhubCache : IPornhubCache
{
    private readonly IMemoryCache _memoryCache;

    public MemoryPornhubCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public ValueTask<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.TryGetValue<string>(key, out var value);
        return ValueTask.FromResult(value);
    }

    public ValueTask SetAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        _memoryCache.Set(key, value, ttl);
        return ValueTask.CompletedTask;
    }
}
