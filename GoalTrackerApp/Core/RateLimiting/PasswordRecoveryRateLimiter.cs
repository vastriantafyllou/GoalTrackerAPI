using System.Collections.Concurrent;

namespace GoalTrackerApp.Core.RateLimiting;

public class PasswordRecoveryRateLimiter
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;

    public PasswordRecoveryRateLimiter(int maxRequests = 3, int timeWindowMinutes = 15)
    {
        _maxRequests = maxRequests;
        _timeWindow = TimeSpan.FromMinutes(timeWindowMinutes);
    }

    public bool IsAllowed(string identifier)
    {
        var now = DateTime.UtcNow;
        var key = identifier.ToLowerInvariant();

        _requests.AddOrUpdate(key,
            new List<DateTime> { now },
            (_, existingRequests) =>
            {
                existingRequests.RemoveAll(time => now - time > _timeWindow);
                existingRequests.Add(now);
                return existingRequests;
            });

        return _requests[key].Count <= _maxRequests;
    }

    public int GetRemainingAttempts(string identifier)
    {
        var key = identifier.ToLowerInvariant();
        if (!_requests.TryGetValue(key, out var requests))
            return _maxRequests;

        var now = DateTime.UtcNow;
        var validRequests = requests.Count(time => now - time <= _timeWindow);
        return Math.Max(0, _maxRequests - validRequests);
    }

    public TimeSpan? GetRetryAfter(string identifier)
    {
        var key = identifier.ToLowerInvariant();
        if (!_requests.TryGetValue(key, out var requests))
            return null;

        var now = DateTime.UtcNow;
        var validRequests = requests.Where(time => now - time <= _timeWindow).OrderBy(time => time).ToList();
        
        if (validRequests.Count < _maxRequests)
            return null;

        var oldestRequest = validRequests.First();
        var retryAfter = _timeWindow - (now - oldestRequest);
        return retryAfter > TimeSpan.Zero ? retryAfter : null;
    }
}
