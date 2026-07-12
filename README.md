# PornhubApiWrapper (.NET class library)

# The Official .NET API Wrapper for the Unofficial Desktop Client
visit `desktophub.app` to learn more.

This is a clean `.NET` wrapper around the public `pornhub.com/webmasters` endpoints.

The goal is simple: make API calls feel like normal C# code, not raw string parsing.

---

## What this library gives you

- Strongly typed requests and responses
- Simple methods for search, trending, stars, tags, categories, and more
- Built-in retry/circuit-breaker support
- Optional caching
- Helpful exceptions when something fails
- Async streaming helpers for pagination (`IAsyncEnumerable<T>`)

---

## Supported endpoints

- `search`
- `video_by_id`
- `is_video_active`
- `stars`
- `categories`
- `tags`
- `channels` (with fallback behavior when unavailable)
- `deleted_videos`

---

## Requirements

- .NET 8 SDK

---

## Add it to your project

If this repo is local, easiest path is a project reference:

```xml
<ProjectReference Include="..\PornhubApiWrapper\PornhubApiWrapper.csproj" />
```


---

## Quick example (manual client)

```csharp
using PornhubApiWrapper;
using PornhubApiWrapper.Requests;

var client = new PornhubClient(new HttpClient(), new PornhubClientOptions
{
    BaseUrl = "https://www.pornhub.com/webmasters/",
    ThrowOnHttpErrors = true,
    Timeout = TimeSpan.FromSeconds(30),
    Resilience = { MaxRetries = 3 },
    Cache = { Enabled = true }
});

var videos = await client.SearchVideosAsync(new SearchVideosRequest
{
    Query = "interview",
    Ordering = VideoOrdering.MostViewed,
    PerPage = 20
});

foreach (var v in videos)
{
    Console.WriteLine($"{v.VideoId} | {v.Title}");
}
```

---

## DI usage (`HttpClientFactory`)

```csharp
using Microsoft.Extensions.DependencyInjection;
using PornhubApiWrapper;
using PornhubApiWrapper.Extensions;

var services = new ServiceCollection();

services.AddPornhubApiWrapper(options =>
{
    options.BaseUrl = "https://www.pornhub.com/webmasters/";
    options.Resilience.MaxRetries = 3;
    options.Cache.Enabled = true;
});

await using var provider = services.BuildServiceProvider();
var client = provider.GetRequiredService<IPornhubClient>();
```

---

## Streaming / auto-pagination example

```csharp
await foreach (var video in client.StreamSearchVideosAsync(
                   new SearchVideosRequest { Query = "interview" },
                   maxItems: 50))
{
    Console.WriteLine(video.Title);
}
```

---

## Error handling (important)

Main exception types:

- `PornhubApiHttpException` -> API returned non-success status
- `PornhubApiValidationException` -> bad request parameters
- `PornhubApiDeserializationException` -> response shape changed / parse failed
- `PornhubApiException` -> base wrapper exception

Example:

```csharp
try
{
    var categories = await client.GetCategoriesAsync();
}
catch (PornhubApiHttpException ex)
{
    Console.WriteLine($"HTTP {(int)ex.StatusCode}: {ex.Message}");
}
```

There is also a non-throwing route:

```csharp
var result = await client.TryGetRawEndpointAsync<Dictionary<string, object>>("categories");
if (!result.IsSuccess)
{
    Console.WriteLine(result.Error?.Message);
}
```

---



## Troubleshooting

- `tags` empty: try broader values (for example `a`) depending on mirror behavior
- `channels` can return `404` in some environments
- If parsing fails suddenly, endpoint payload likely changed; inspect raw endpoint output and update models

---

## Legal note

Public endpoint behavior can change at any time.  
Use this library responsibly and follow local laws/platform policies for your region.
