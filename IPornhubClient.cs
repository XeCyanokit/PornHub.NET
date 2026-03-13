using PornhubApiWrapper.Models;
using PornhubApiWrapper.Requests;
using PornhubApiWrapper.Results;

namespace PornhubApiWrapper;

/// <summary>
/// Main abstraction for interacting with public Pornhub Webmasters endpoints.
/// </summary>
public interface IPornhubClient
{
    /// <summary>Raised after every API response (including from cache). Use for developer mode or logging.</summary>
    event EventHandler<ApiResponseEventArgs>? RawResponseReceived;

    /// <summary>Returns the raw JSON response for an endpoint without deserializing.</summary>
    Task<string?> GetRawResponseAsync(string endpoint, IReadOnlyDictionary<string, string?>? query = null, CancellationToken cancellationToken = default);

    /// <summary>Searches videos using filters and pagination.</summary>
    Task<IReadOnlyList<VideoSummary>> SearchVideosAsync(SearchVideosRequest request, CancellationToken cancellationToken = default);
    /// <summary>Searches videos and returns the list plus total count when the API provides it.</summary>
    Task<SearchVideosResult> SearchVideosWithTotalAsync(SearchVideosRequest request, CancellationToken cancellationToken = default);
    /// <summary>Returns trending videos.</summary>
    Task<IReadOnlyList<VideoSummary>> GetTrendingVideosAsync(TrendingVideosRequest? request = null, CancellationToken cancellationToken = default);
    /// <summary>Fetches details for a single video ID.</summary>
    Task<VideoDetails?> GetVideoByIdAsync(string videoId, CancellationToken cancellationToken = default);
    /// <summary>Checks whether a video is currently active.</summary>
    Task<bool> IsVideoActiveAsync(string videoId, CancellationToken cancellationToken = default);
    /// <summary>Lists actresses/stars.</summary>
    Task<IReadOnlyList<Pornstar>> GetActressesAsync(ActressSearchRequest? request = null, CancellationToken cancellationToken = default);
    /// <summary>Lists categories.</summary>
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    /// <summary>Lists tags.</summary>
    Task<IReadOnlyList<Tag>> GetTagsAsync(TagSearchRequest? request = null, CancellationToken cancellationToken = default);
    /// <summary>Lists channels when endpoint is available.</summary>
    Task<IReadOnlyList<Channel>> GetChannelsAsync(ChannelSearchRequest? request = null, CancellationToken cancellationToken = default);
    /// <summary>Lists deleted videos.</summary>
    Task<IReadOnlyList<VideoSummary>> GetDeletedVideosAsync(DeletedVideosRequest? request = null, CancellationToken cancellationToken = default);
    /// <summary>Calls a raw endpoint and deserializes into the requested type.</summary>
    Task<T?> GetRawEndpointAsync<T>(string endpoint, IReadOnlyDictionary<string, string?>? query = null, CancellationToken cancellationToken = default);
    /// <summary>Non-throwing version of <see cref="GetRawEndpointAsync{T}"/>.</summary>
    Task<ApiResult<T?>> TryGetRawEndpointAsync<T>(string endpoint, IReadOnlyDictionary<string, string?>? query = null, CancellationToken cancellationToken = default);

    /// <summary>Streams paginated video search results.</summary>
    IAsyncEnumerable<VideoSummary> StreamSearchVideosAsync(SearchVideosRequest request, int? maxItems = null, CancellationToken cancellationToken = default);
    /// <summary>Streams paginated trending video results.</summary>
    IAsyncEnumerable<VideoSummary> StreamTrendingVideosAsync(TrendingVideosRequest? request = null, int? maxItems = null, CancellationToken cancellationToken = default);
    /// <summary>Streams paginated actress results.</summary>
    IAsyncEnumerable<Pornstar> StreamActressesAsync(ActressSearchRequest? request = null, int? maxItems = null, CancellationToken cancellationToken = default);
    /// <summary>Streams paginated tag results.</summary>
    IAsyncEnumerable<Tag> StreamTagsAsync(TagSearchRequest? request = null, int? maxItems = null, CancellationToken cancellationToken = default);
    /// <summary>Streams paginated deleted video results.</summary>
    IAsyncEnumerable<VideoSummary> StreamDeletedVideosAsync(DeletedVideosRequest? request = null, int? maxItems = null, CancellationToken cancellationToken = default);
}
