using System.Net;

namespace PornhubApiWrapper.Exceptions;

public class PornhubApiException : Exception
{
    public PornhubApiException(string message, string endpoint, HttpStatusCode? statusCode = null, string? responseBody = null, bool isTransient = false, Exception? innerException = null)
        : base(message, innerException)
    {
        Endpoint = endpoint;
        StatusCode = statusCode;
        ResponseBody = responseBody;
        IsTransient = isTransient;
    }

    public string Endpoint { get; }

    public HttpStatusCode? StatusCode { get; }

    public string? ResponseBody { get; }

    public bool IsTransient { get; }
}
