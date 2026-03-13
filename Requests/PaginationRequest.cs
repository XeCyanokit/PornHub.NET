namespace PornhubApiWrapper.Requests;

public abstract class PaginationRequest
{
    public int? Page { get; set; }
    public int? PerPage { get; set; }
}
