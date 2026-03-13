namespace PornhubApiWrapper.Requests;

public sealed class SearchVideosRequest : PaginationRequest
{
    /// <summary>Free-text search query.</summary>
    public string? Query { get; set; }
    /// <summary>Exact phrase search (when supported by API).</summary>
    public string? Phrase { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public string? Stars { get; set; }
    public string? Channel { get; set; }
    public string? Production { get; set; }
    public string? Period { get; set; }
    public string? ThumbSize { get; set; }
    public bool? IsHd { get; set; }
    public bool? IsPremium { get; set; }
    public VideoOrdering Ordering { get; set; } = VideoOrdering.Newest;
}
