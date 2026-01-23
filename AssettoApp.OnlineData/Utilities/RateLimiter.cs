namespace AssettoApp.OnlineData.Utilities;

/// <summary>
/// Rate limiter to respect API/website limits
/// </summary>
public class RateLimiter
{
    private readonly int _minDelayMs;
    private readonly int _maxRequestsPerMinute;
    private readonly Queue<DateTime> _requestTimestamps = new();
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly object _lock = new();

    public RateLimiter(int minDelayMs, int maxRequestsPerMinute)
    {
        _minDelayMs = minDelayMs;
        _maxRequestsPerMinute = maxRequestsPerMinute;
    }

    /// <summary>
    /// Wait if necessary to respect rate limits, then record the request
    /// </summary>
    public async Task WaitIfNeededAsync(CancellationToken cancellationToken = default)
    {
        int delayMs = 0;
        
        lock (_lock)
        {
            var now = DateTime.UtcNow;

            // Ensure minimum delay between requests
            var timeSinceLastRequest = now - _lastRequestTime;
            if (timeSinceLastRequest.TotalMilliseconds < _minDelayMs)
            {
                delayMs = _minDelayMs - (int)timeSinceLastRequest.TotalMilliseconds;
            }

            // Remove timestamps older than 1 minute
            var oneMinuteAgo = now.AddMinutes(-1);
            while (_requestTimestamps.Count > 0 && _requestTimestamps.Peek() < oneMinuteAgo)
            {
                _requestTimestamps.Dequeue();
            }

            // Check if we've exceeded requests per minute
            if (_requestTimestamps.Count >= _maxRequestsPerMinute)
            {
                var oldestRequest = _requestTimestamps.Peek();
                var waitTime = (int)(60000 - (now - oldestRequest).TotalMilliseconds);
                if (waitTime > delayMs)
                {
                    delayMs = waitTime;
                }
            }
        }

        // Perform the actual delay outside the lock
        if (delayMs > 0)
        {
            await Task.Delay(delayMs, cancellationToken);
        }

        // Record the request after delay
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            _requestTimestamps.Enqueue(now);
            _lastRequestTime = now;
            
            // Clean up old timestamps again
            var oneMinuteAgo = now.AddMinutes(-1);
            while (_requestTimestamps.Count > 0 && _requestTimestamps.Peek() < oneMinuteAgo)
            {
                _requestTimestamps.Dequeue();
            }
        }
    }

    /// <summary>
    /// Get the number of requests made in the last minute
    /// </summary>
    public int GetRequestCountLastMinute()
    {
        lock (_lock)
        {
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            while (_requestTimestamps.Count > 0 && _requestTimestamps.Peek() < oneMinuteAgo)
            {
                _requestTimestamps.Dequeue();
            }
            return _requestTimestamps.Count;
        }
    }
}
