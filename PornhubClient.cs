using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using PornhubApiWrapper.Caching;
using PornhubApiWrapper.Exceptions;
using PornhubApiWrapper.Internal;
using PornhubApiWrapper.Models;
using PornhubApiWrapper.Requests;
using PornhubApiWrapper.Resilience;
using PornhubApiWrapper.Results;
using PornhubApiWrapper.Validation;

namespace PornhubApiWrapper;

public sealed class PornhubClient : IPornhubClient
{
    private readonly HttpClient _httpClient;
    private readonly PornhubClientOptions _options;
    private readonly IPornhubCache _cache;
    private readonly RequestValidator _validator;
    private readonly SimpleCircuitBreaker _breaker;
    private readonly JsonSerializerOptions _jsonOptions;

    public PornhubClient(HttpClient httpClient, PornhubClientOptions? options = null)
        : this(httpClient, options, null)
    {
    }

    public PornhubClient(HttpClient httpClient, PornhubClientOptions? options, IPornhubCache? cache)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? new PornhubClientOptions();
        _cache = cache ?? NullPornhubCache.Instance;
        _validator = new RequestValidator(_options);
        _breaker = new SimpleCircuitBreaker(_options.Resilience.CircuitBreakerFailureThreshold, _options.Resilience.CircuitBreakerDuration);

        RawResponseReceived = null;

        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl, UriKind.Absolute);
        }

        if (_httpClient.Timeout == Timeout.InfiniteTimeSpan || _httpClient.Timeout > _options.Timeout)
        {
            _httpClient.Timeout = _options.Timeout;
        }

        if (!string.IsNullOrWhiteSpace(_options.UserAgent))
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IReadOnlyList<VideoSummary>> SearchVideosAsync(SearchVideosRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        _validator.ValidatePagination("search", request);

        var query = BuildSearchQuery(request);
        AddPagination(query, request, _options);

        var payload = await SendAsync<VideoCollectionResponse>("search", query, cancellationToken).ConfigureAwait(false);
        return payload?.Videos ?? [];
    }

    public async Task<SearchVideosResult> SearchVideosWithTotalAsync(SearchVideosRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        _validator.ValidatePagination("search", request);
        var query = BuildSearchQuery(request);
        AddPagination(query, request, _options);
        var payload = await SendAsync<VideoCollectionResponse>("search", query, cancellationToken).ConfigureAwait(false);
        var videos = payload?.Videos ?? [];
        var total = payload?.Total ?? payload?.Count;
        return new SearchVideosResult { Videos = videos, TotalCount = total };
    }

    public event EventHandler<ApiResponseEventArgs>? RawResponseReceived;

    public async Task<string?> GetRawResponseAsync(string endpoint, IReadOnlyDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        _validator.ValidateEndpoint(endpoint);
        Dictionary<string, string?>? mutable = null;
        if (query != null)
            mutable = query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        return await GetPayloadTextAsync(endpoint, mutable, cancellationToken).ConfigureAwait(false);
    }

    private static Dictionary<string, string?> BuildSearchQuery(SearchVideosRequest request)
    {
        var query = new Dictionary<string, string?>
        {
            ["search"] = request.Query,
            ["phrase"] = request.Phrase,
            ["category"] = request.Category,
            ["tags"] = request.Tags,
            ["stars"] = request.Stars,
            ["channel"] = request.Channel,
            ["channels"] = request.Channel,
            ["production"] = request.Production,
            ["period"] = request.Period,
            ["thumbsize"] = request.ThumbSize,
            ["ordering"] = ToOrderingValue(request.Ordering),
            ["hd"] = BoolAsFlag(request.IsHd),
            ["premium"] = BoolAsFlag(request.IsPremium)
        };
        return query;
    }

    public Task<IReadOnlyList<VideoSummary>> GetTrendingVideosAsync(TrendingVideosRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new TrendingVideosRequest();
        var search = new SearchVideosRequest
        {
            Category = request.Category,
            Ordering = VideoOrdering.Trending,
            Period = request.Period,
            ThumbSize = request.ThumbSize,
            Page = request.Page,
            PerPage = request.PerPage
        };

        return SearchVideosAsync(search, cancellationToken);
    }

    public async Task<VideoDetails?> GetVideoByIdAsync(string videoId, CancellationToken cancellationToken = default)
    {
        _validator.ValidateVideoId("video_by_id", videoId);

        var payload = await SendAsync<VideoDetailsResponse>(
            endpoint: "video_by_id",
            query: new Dictionary<string, string?> { ["id"] = videoId },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return payload?.Video;
    }

    public async Task<bool> IsVideoActiveAsync(string videoId, CancellationToken cancellationToken = default)
    {
        _validator.ValidateVideoId("is_video_active", videoId);

        var payload = await SendAsync<VideoActiveResponse>(
            endpoint: "is_video_active",
            query: new Dictionary<string, string?> { ["id"] = videoId },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return payload?.Active?.IsActive is "1" or "true";
    }

    public async Task<IReadOnlyList<Pornstar>> GetActressesAsync(ActressSearchRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new ActressSearchRequest();
        _validator.ValidatePagination("stars", request);

        var query = new Dictionary<string, string?>
        {
            ["search"] = request.Name,
            ["country"] = request.Country,
            ["sort"] = request.Sort
        };

        AddPagination(query, request, _options);

        var payload = await SendAsync<StarsResponse>("stars", query, cancellationToken).ConfigureAwait(false);
        if (payload?.Stars is null || payload.Stars.Count == 0)
        {
            return [];
        }

        var stars = payload.Stars
            .Select(s =>
            {
                if (s.Star != null)
                    return s.Star;
                var name = s.StarName ?? s.PornstarName;
                if (string.IsNullOrEmpty(name)) return null;
                return new Pornstar
                {
                    Name = name,
                    AlternateName = s.StarName,
                    Url = s.StarUrl ?? s.PornstarUrl,
                    AlternateUrl = s.StarUrl ?? s.PornstarUrl,
                    ThumbnailUrl = s.PornstarThumb ?? s.StarThumb,
                    AlternateThumbnailUrl = s.StarThumb ?? s.PornstarThumb
                };
            })
            .Where(s => s is not null)
            .Select(s => s!)
            .ToList();

        return stars;
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var payload = await SendAsync<CategoriesResponse>("categories", null, cancellationToken).ConfigureAwait(false);
        return payload?.Categories ?? [];
    }

    public async Task<IReadOnlyList<Tag>> GetTagsAsync(TagSearchRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new TagSearchRequest();
        _validator.ValidateTagRequest(request);
        _validator.ValidatePagination("tags", request);
        var query = new Dictionary<string, string?> { ["list"] = request.Name };
        AddPagination(query, request, _options);

        var payload = await SendAsync<TagsResponse>("tags", query, cancellationToken).ConfigureAwait(false);
        return ParseTags(payload?.Tags);
    }

    public async Task<IReadOnlyList<Channel>> GetChannelsAsync(ChannelSearchRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new ChannelSearchRequest();
        _validator.ValidatePagination("channels", request);
        var query = new Dictionary<string, string?> { ["search"] = request.Name };
        AddPagination(query, request, _options);

        try
        {
            var payload = await SendAsync<ChannelsResponse>("channels", query, cancellationToken).ConfigureAwait(false);
            return payload?.Channels ?? [];
        }
        catch (PornhubApiHttpException ex) when (_options.EnableCompatibilityFallbacks && ex.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<VideoSummary>> GetDeletedVideosAsync(DeletedVideosRequest? request = null, CancellationToken cancellationToken = default)
    {
        request ??= new DeletedVideosRequest();
        _validator.ValidatePagination("deleted_videos", request);
        var query = new Dictionary<string, string?> { ["type"] = request.Type };
        AddPagination(query, request, _options);

        var payload = await SendAsync<VideoCollectionResponse>("deleted_videos", query, cancellationToken).ConfigureAwait(false);
        return payload?.Videos ?? [];
    }

    public Task<T?> GetRawEndpointAsync<T>(string endpoint, IReadOnlyDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        _validator.ValidateEndpoint(endpoint);

        Dictionary<string, string?>? mutable = null;
        if (query is not null)
        {
            mutable = query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        return SendAsync<T>(endpoint, mutable, cancellationToken);
    }

    public async Task<ApiResult<T?>> TryGetRawEndpointAsync<T>(string endpoint, IReadOnlyDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await GetRawEndpointAsync<T>(endpoint, query, cancellationToken).ConfigureAwait(false);
            return ApiResult<T?>.Success(value);
        }
        catch (Exception ex)
        {
            return ApiResult<T?>.Failure(ex);
        }
    }

    public async IAsyncEnumerable<VideoSummary> StreamSearchVideosAsync(SearchVideosRequest request, int? maxItems = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request ??= new SearchVideosRequest();
        var current = request.Page ?? _options.Pagination.DefaultPage;
        var perPage = request.PerPage ?? _options.Pagination.DefaultPerPage;
        var yielded = 0;
        var cap = maxItems ?? int.MaxValue;

        while (yielded < cap)
        {
            var pageRequest = new SearchVideosRequest
            {
                Query = request.Query,
                Category = request.Category,
                Tags = request.Tags,
                Stars = request.Stars,
                Production = request.Production,
                Period = request.Period,
                ThumbSize = request.ThumbSize,
                IsHd = request.IsHd,
                IsPremium = request.IsPremium,
                Ordering = request.Ordering,
                Page = current,
                PerPage = perPage
            };

            var pageItems = await SearchVideosAsync(pageRequest, cancellationToken).ConfigureAwait(false);
            if (pageItems.Count == 0)
            {
                yield break;
            }

            foreach (var item in pageItems)
            {
                if (yielded++ >= cap)
                {
                    yield break;
                }

                yield return item;
            }

            current++;
        }
    }

    public IAsyncEnumerable<VideoSummary> StreamTrendingVideosAsync(TrendingVideosRequest? request = null, int? maxItems = null, CancellationToken cancellationToken = default)
    {
        request ??= new TrendingVideosRequest();
        var search = new SearchVideosRequest
        {
            Category = request.Category,
            Ordering = VideoOrdering.Trending,
            Period = request.Period,
            ThumbSize = request.ThumbSize,
            Page = request.Page,
            PerPage = request.PerPage
        };

        return StreamSearchVideosAsync(search, maxItems, cancellationToken);
    }

    public async IAsyncEnumerable<Pornstar> StreamActressesAsync(ActressSearchRequest? request = null, int? maxItems = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request ??= new ActressSearchRequest();
        var current = request.Page ?? _options.Pagination.DefaultPage;
        var perPage = request.PerPage ?? _options.Pagination.DefaultPerPage;
        var yielded = 0;
        var cap = maxItems ?? int.MaxValue;

        while (yielded < cap)
        {
            var pageRequest = new ActressSearchRequest
            {
                Name = request.Name,
                Country = request.Country,
                Page = current,
                PerPage = perPage
            };

            var pageItems = await GetActressesAsync(pageRequest, cancellationToken).ConfigureAwait(false);
            if (pageItems.Count == 0)
            {
                yield break;
            }

            foreach (var item in pageItems)
            {
                if (yielded++ >= cap)
                {
                    yield break;
                }

                yield return item;
            }

            current++;
        }
    }

    public async IAsyncEnumerable<Tag> StreamTagsAsync(TagSearchRequest? request = null, int? maxItems = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request ??= new TagSearchRequest { Name = "a" };
        var current = request.Page ?? _options.Pagination.DefaultPage;
        var perPage = request.PerPage ?? _options.Pagination.DefaultPerPage;
        var yielded = 0;
        var cap = maxItems ?? int.MaxValue;

        while (yielded < cap)
        {
            var pageRequest = new TagSearchRequest
            {
                Name = request.Name,
                Page = current,
                PerPage = perPage
            };

            var pageItems = await GetTagsAsync(pageRequest, cancellationToken).ConfigureAwait(false);
            if (pageItems.Count == 0)
            {
                yield break;
            }

            foreach (var item in pageItems)
            {
                if (yielded++ >= cap)
                {
                    yield break;
                }

                yield return item;
            }

            current++;
        }
    }

    public async IAsyncEnumerable<VideoSummary> StreamDeletedVideosAsync(DeletedVideosRequest? request = null, int? maxItems = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request ??= new DeletedVideosRequest();
        var current = request.Page ?? _options.Pagination.DefaultPage;
        var perPage = request.PerPage ?? _options.Pagination.DefaultPerPage;
        var yielded = 0;
        var cap = maxItems ?? int.MaxValue;

        while (yielded < cap)
        {
            var pageRequest = new DeletedVideosRequest
            {
                Type = request.Type,
                Page = current,
                PerPage = perPage
            };

            var pageItems = await GetDeletedVideosAsync(pageRequest, cancellationToken).ConfigureAwait(false);
            if (pageItems.Count == 0)
            {
                yield break;
            }

            foreach (var item in pageItems)
            {
                if (yielded++ >= cap)
                {
                    yield break;
                }

                yield return item;
            }

            current++;
        }
    }

    private async Task<string?> GetPayloadTextAsync(string endpoint, Dictionary<string, string?>? query, CancellationToken cancellationToken)
    {
        var requestUri = BuildEndpointUri(endpoint, query);
        var cacheKey = BuildCacheKey(endpoint, query);
        if (_options.Cache.Enabled && !string.IsNullOrWhiteSpace(cacheKey))
        {
            var cachedPayload = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(cachedPayload))
            {
                RaiseRawResponseReceived(endpoint, query, cachedPayload);
                return cachedPayload;
            }
        }

        var payloadText = await SendWithResilienceAsync(endpoint, requestUri, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(payloadText))
            return null;

        if (_options.Cache.Enabled && !string.IsNullOrWhiteSpace(cacheKey))
        {
            var ttl = ResolveCacheTtl(endpoint);
            await _cache.SetAsync(cacheKey, payloadText, ttl, cancellationToken).ConfigureAwait(false);
        }

        RaiseRawResponseReceived(endpoint, query, payloadText);
        return payloadText;
    }

    private void RaiseRawResponseReceived(string endpoint, Dictionary<string, string?>? query, string rawBody)
    {
        var queryString = BuildQueryString(query);
        RawResponseReceived?.Invoke(this, new ApiResponseEventArgs(endpoint, string.IsNullOrWhiteSpace(queryString) ? null : queryString, rawBody, DateTimeOffset.UtcNow));
    }

    private async Task<T?> SendAsync<T>(string endpoint, Dictionary<string, string?>? query, CancellationToken cancellationToken)
    {
        _validator.ValidateEndpoint(endpoint);
        var payloadText = await GetPayloadTextAsync(endpoint, query, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(payloadText))
            return default;
        return Deserialize<T>(endpoint, payloadText);
    }

    private Uri BuildEndpointUri(string endpoint, Dictionary<string, string?>? query)
    {
        var normalized = endpoint.Trim().TrimStart('/');
        var queryString = BuildQueryString(query);
        var relative = string.IsNullOrWhiteSpace(queryString) ? normalized : $"{normalized}?{queryString}";
        return new Uri(_httpClient.BaseAddress!, relative);
    }

    private static string BuildQueryString(Dictionary<string, string?>? query)
    {
        if (query is null || query.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var (key, value) in query)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value));
        }

        return builder.ToString();
    }

    private static void AddPagination(Dictionary<string, string?> query, PaginationRequest request, PornhubClientOptions options)
    {
        var page = request.Page ?? options.Pagination.DefaultPage;
        var perPage = request.PerPage ?? options.Pagination.DefaultPerPage;
        perPage = Math.Min(perPage, options.Pagination.MaxPerPage);

        query["page"] = page.ToString(CultureInfo.InvariantCulture);
        query["per_page"] = perPage.ToString(CultureInfo.InvariantCulture);
    }

    private static string? BoolAsFlag(bool? value)
    {
        return value switch
        {
            true => "1",
            false => "0",
            _ => null
        };
    }

    private static string ToOrderingValue(VideoOrdering ordering)
    {
        return ordering switch
        {
            VideoOrdering.Newest => "newest",
            VideoOrdering.MostViewed => "mostviewed",
            VideoOrdering.Rating => "rating",
            VideoOrdering.Longest => "longest",
            VideoOrdering.Trending => "trending",
            _ => "newest"
        };
    }

    private static IReadOnlyList<Tag> ParseTags(JsonElement? tagsElement)
    {
        if (tagsElement is null || tagsElement.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return [];
        }

        if (tagsElement.Value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var tags = new List<Tag>();
        foreach (var item in tagsElement.Value.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                tags.Add(new Tag { Name = item.GetString() });
                continue;
            }

            if (item.ValueKind == JsonValueKind.Object)
            {
                if (item.TryGetProperty("tag_name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
                {
                    var tag = new Tag
                    {
                        Name = nameElement.GetString()
                    };

                    if (item.TryGetProperty("tag_url", out var urlElement) && urlElement.ValueKind == JsonValueKind.String)
                        tag.Url = urlElement.GetString();
                    if (item.TryGetProperty("tag_thumb", out var thumbElement) && thumbElement.ValueKind == JsonValueKind.String)
                        tag.ThumbnailUrl = thumbElement.GetString();

                    tags.Add(tag);
                }
            }
        }

        return tags;
    }

    private T? Deserialize<T>(string endpoint, string payloadText)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payloadText, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new PornhubApiDeserializationException(endpoint, payloadText, ex);
        }
    }

    private async Task<string?> SendWithResilienceAsync(string endpoint, Uri requestUri, CancellationToken cancellationToken)
    {
        if (_breaker.IsOpen())
        {
            throw new PornhubApiException($"Circuit is open for endpoint '{endpoint}'.", endpoint, isTransient: true);
        }

        var maxRetries = Math.Max(0, _options.Resilience.MaxRetries);
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
                var payloadText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    _breaker.RegisterSuccess();
                    return payloadText;
                }

                var isTransient = TransientFailureClassifier.IsTransient(response.StatusCode, _options.Resilience);
                if (attempt < maxRetries && isTransient)
                {
                    await DelayForRetryAsync(attempt, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                _breaker.RegisterFailure();
                if (!_options.ThrowOnHttpErrors)
                {
                    return null;
                }

                throw new PornhubApiHttpException(endpoint, response.StatusCode, payloadText, isTransient);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                var isTransient = true;
                if (attempt < maxRetries)
                {
                    await DelayForRetryAsync(attempt, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                _breaker.RegisterFailure();
                throw new PornhubApiException($"Request timed out for endpoint '{endpoint}'.", endpoint, isTransient: isTransient, innerException: ex);
            }
            catch (HttpRequestException ex)
            {
                var isTransient = true;
                if (attempt < maxRetries)
                {
                    await DelayForRetryAsync(attempt, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                _breaker.RegisterFailure();
                throw new PornhubApiException($"Transport error for endpoint '{endpoint}'.", endpoint, ex.StatusCode, isTransient: isTransient, innerException: ex);
            }
        }
    }

    private Task DelayForRetryAsync(int attempt, CancellationToken cancellationToken)
    {
        var delayMs = _options.Resilience.BaseRetryDelay.TotalMilliseconds * Math.Pow(2, attempt);
        var jitterMs = Random.Shared.Next(25, 151);
        return Task.Delay(TimeSpan.FromMilliseconds(delayMs + jitterMs), cancellationToken);
    }

    private string BuildCacheKey(string endpoint, Dictionary<string, string?>? query)
    {
        var queryString = BuildQueryString(query);
        return string.IsNullOrWhiteSpace(queryString) ? endpoint.Trim() : $"{endpoint.Trim()}?{queryString}";
    }

    private TimeSpan ResolveCacheTtl(string endpoint)
    {
        if (_options.Cache.EndpointTtls.TryGetValue(endpoint, out var ttl))
        {
            return ttl;
        }

        return _options.Cache.DefaultTtl;
    }
}
