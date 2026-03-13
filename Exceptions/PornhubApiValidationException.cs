namespace PornhubApiWrapper.Exceptions;

public sealed class PornhubApiValidationException : PornhubApiException
{
    public PornhubApiValidationException(string endpoint, string message)
        : base(message, endpoint)
    {
    }
}
