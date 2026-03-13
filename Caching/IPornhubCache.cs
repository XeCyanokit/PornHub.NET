namespace PornhubApiWrapper.Caching;

public interface IPornhubCache
{
    ValueTask<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    ValueTask SetAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken = default);
}
