namespace PornhubApiWrapper.Requests;

public sealed class DeletedVideosRequest : PaginationRequest
{
    public string? Type { get; set; }
}
