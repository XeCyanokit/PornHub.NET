using PornhubApiWrapper.Models;

namespace PornhubApiWrapper.Results;

/// <summary>Search result with optional total count when the API provides it.</summary>
public sealed class SearchVideosResult
{
    public IReadOnlyList<VideoSummary> Videos { get; init; } = [];
    public long? TotalCount { get; init; }
}
