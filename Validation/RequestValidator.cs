using PornhubApiWrapper.Exceptions;
using PornhubApiWrapper.Requests;

namespace PornhubApiWrapper.Validation;

internal sealed class RequestValidator
{
    private readonly PornhubClientOptions _options;

    public RequestValidator(PornhubClientOptions options)
    {
        _options = options;
    }

    public void ValidateEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new PornhubApiValidationException("n/a", "Endpoint cannot be null or whitespace.");
        }
    }

    public void ValidateVideoId(string endpoint, string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new PornhubApiValidationException(endpoint, "A non-empty video id is required.");
        }
    }

    public void ValidatePagination(string endpoint, PaginationRequest request)
    {
        if (request.Page is <= 0)
        {
            throw new PornhubApiValidationException(endpoint, "Page must be greater than zero.");
        }

        if (request.PerPage is <= 0)
        {
            throw new PornhubApiValidationException(endpoint, "PerPage must be greater than zero.");
        }

        if (request.PerPage > _options.Pagination.MaxPerPage)
        {
            throw new PornhubApiValidationException(endpoint, $"PerPage cannot exceed {_options.Pagination.MaxPerPage}.");
        }
    }

    public void ValidateTagRequest(TagSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new PornhubApiValidationException("tags", "Name is required for tags. Use at least one character.");
        }
    }
}
