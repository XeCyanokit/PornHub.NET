namespace PornhubApiWrapper;

/// <summary>Provides the raw API response for developer mode or logging.</summary>
public sealed class ApiResponseEventArgs : EventArgs
{
    public string Endpoint { get; }
    public string? QueryString { get; }
    public string RawBody { get; }
    public DateTimeOffset TimestampUtc { get; }

    public ApiResponseEventArgs(string endpoint, string? queryString, string rawBody, DateTimeOffset timestampUtc)
    {
        Endpoint = endpoint ?? "";
        QueryString = queryString;
        RawBody = rawBody ?? "";
        TimestampUtc = timestampUtc;
    }
}
