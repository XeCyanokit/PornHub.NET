using System.Net;

namespace PornhubApiWrapper.Exceptions;

public sealed class PornhubApiHttpException : PornhubApiException
{
    public PornhubApiHttpException(string endpoint, HttpStatusCode statusCode, string? responseBody, bool isTransient)
        : base(
            message: $"Pornhub API request failed for endpoint '{endpoint}' with status {(int)statusCode} ({statusCode}).",
            endpoint: endpoint,
            statusCode: statusCode,
            responseBody: responseBody,
            isTransient: isTransient)
    {
    }
}
