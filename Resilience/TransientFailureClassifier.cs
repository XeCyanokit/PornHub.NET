using System.Net;

namespace PornhubApiWrapper.Resilience;

internal static class TransientFailureClassifier
{
    public static bool IsTransient(HttpStatusCode statusCode, ResilienceOptions options)
    {
        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            return options.RetryOn429;
        }

        if (statusCode == HttpStatusCode.RequestTimeout)
        {
            return options.RetryOn408;
        }

        return options.RetryOn5xx && (int)statusCode >= 500;
    }
}
