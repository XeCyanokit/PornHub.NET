namespace PornhubApiWrapper.Requests;

public sealed class TrendingVideosRequest : PaginationRequest
{
    public string? Period { get; set; } = "weekly";
    public string? Category { get; set; }
    public string? ThumbSize { get; set; }
}
