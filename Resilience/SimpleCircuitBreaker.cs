namespace PornhubApiWrapper.Resilience;

internal sealed class SimpleCircuitBreaker
{
    private readonly object _sync = new();
    private readonly int _failureThreshold;
    private readonly TimeSpan _openDuration;
    private int _consecutiveFailures;
    private DateTimeOffset? _openUntilUtc;

    public SimpleCircuitBreaker(int failureThreshold, TimeSpan openDuration)
    {
        _failureThreshold = Math.Max(1, failureThreshold);
        _openDuration = openDuration <= TimeSpan.Zero ? TimeSpan.FromSeconds(20) : openDuration;
    }

    public bool IsOpen()
    {
        lock (_sync)
        {
            if (_openUntilUtc is null)
            {
                return false;
            }

            if (DateTimeOffset.UtcNow >= _openUntilUtc.Value)
            {
                _openUntilUtc = null;
                _consecutiveFailures = 0;
                return false;
            }

            return true;
        }
    }

    public void RegisterSuccess()
    {
        lock (_sync)
        {
            _consecutiveFailures = 0;
            _openUntilUtc = null;
        }
    }

    public void RegisterFailure()
    {
        lock (_sync)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= _failureThreshold)
            {
                _openUntilUtc = DateTimeOffset.UtcNow.Add(_openDuration);
                _consecutiveFailures = 0;
            }
        }
    }
}
