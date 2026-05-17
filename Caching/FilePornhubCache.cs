using System.Security.Cryptography;
using System.Text;

namespace PornhubApiWrapper.Caching;

/// <summary>Disk-backed cache for raw JSON API payloads (one file per entry under a root folder).</summary>
public sealed class FilePornhubCache : IPornhubCache, IDisposable
{
    private readonly string _root;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public FilePornhubCache(string rootDirectory)
    {
        _root = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        Directory.CreateDirectory(_root);
    }

    public async ValueTask<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key)) return null;
        var basePath = GetPaths(key);
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(basePath.PayloadPath)) return null;
            if (!File.Exists(basePath.ExpiryPath)) return null;
            var expText = await File.ReadAllTextAsync(basePath.ExpiryPath, cancellationToken).ConfigureAwait(false);
            if (!long.TryParse(expText.Trim(), out var ticks)) return null;
            var exp = new DateTime(ticks, DateTimeKind.Utc);
            if (DateTime.UtcNow >= exp)
            {
                TryDeletePair(basePath);
                return null;
            }
            return await File.ReadAllTextAsync(basePath.PayloadPath, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask SetAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;
        var basePath = GetPaths(key);
        var exp = DateTime.UtcNow.Add(ttl);
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var dir = Path.GetDirectoryName(basePath.PayloadPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            await File.WriteAllTextAsync(basePath.PayloadPath, value, cancellationToken).ConfigureAwait(false);
            await File.WriteAllTextAsync(basePath.ExpiryPath, exp.Ticks.ToString(), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    private (string PayloadPath, string ExpiryPath) GetPaths(string key)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();
        var sub = hash[..2];
        var folder = Path.Combine(_root, sub);
        var baseName = hash;
        return (Path.Combine(folder, baseName + ".txt"), Path.Combine(folder, baseName + ".exp"));
    }

    private static void TryDeletePair((string PayloadPath, string ExpiryPath) basePath)
    {
        try { if (File.Exists(basePath.PayloadPath)) File.Delete(basePath.PayloadPath); } catch { }
        try { if (File.Exists(basePath.ExpiryPath)) File.Delete(basePath.ExpiryPath); } catch { }
    }

    public void Dispose() => _lock.Dispose();
}
