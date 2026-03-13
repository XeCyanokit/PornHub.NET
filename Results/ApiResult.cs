namespace PornhubApiWrapper.Results;

public sealed class ApiResult<T>
{
    private ApiResult(T? value, Exception? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public Exception? Error { get; }

    public bool IsSuccess => Error is null;

    public static ApiResult<T> Success(T? value) => new(value, null);

    public static ApiResult<T> Failure(Exception error) => new(default, error);
}
