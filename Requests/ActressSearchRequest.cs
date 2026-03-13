namespace PornhubApiWrapper.Requests;

public sealed class ActressSearchRequest : PaginationRequest
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    /// <summary>Sort order: view, trend, subs, alpha, videos, random (when supported by API).</summary>
    public string? Sort { get; set; }
}
