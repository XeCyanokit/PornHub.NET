namespace PornhubApiWrapper.Exceptions;

public sealed class PornhubApiDeserializationException : PornhubApiException
{
    public PornhubApiDeserializationException(string endpoint, string responseBody, Exception innerException)
        : base(
            message: $"Pornhub API response parsing failed for endpoint '{endpoint}'.",
            endpoint: endpoint,
            responseBody: responseBody,
            innerException: innerException)
    {
    }
}
