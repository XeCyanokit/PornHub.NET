namespace PornhubApiWrapper.Caching;

public sealed class NullPornhubCache : IPornhubCache
{
    public static readonly NullPornhubCache Instance = new();

    private NullPornhubCache()
    {
    }

    public ValueTask<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<string?>(null);
    }

    public ValueTask SetAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}
