using System.Collections.Concurrent;
using System.Net;

namespace Lunatune.Api.Middleware;

public class UserBlockingMiddleware(RequestDelegate next)
{
    private static readonly ConcurrentDictionary<string, (int ViolationCount, DateTime? BlockedUntil)> _userViolations = new();
    private const int MaxViolations = 5;
    private static readonly TimeSpan BlockDuration = TimeSpan.FromHours(1);

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User?.Identity?.Name;
        if (userId == null)
        {
            await next(context);
            return;
        }

        // Check if user is blocked
        if (_userViolations.TryGetValue(userId, out var userState))
        {
            var (violationCount, blockedUntil) = userState;

            // If block period has expired, remove the user from violations
            if (blockedUntil.HasValue && DateTime.UtcNow >= blockedUntil.Value)
            {
                _userViolations.TryRemove(userId, out _);
            }
            else if (blockedUntil.HasValue && DateTime.UtcNow < blockedUntil.Value)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.RetryAfter = (blockedUntil.Value - DateTime.UtcNow).TotalSeconds.ToString();
                return;
            }
        }

        // Process the request
        await next(context);

        // Check if the request was rate limited (20 requests per minute)
        if (context.Response.StatusCode == 429)
        {
            var currentState = _userViolations.AddOrUpdate(
                userId,
                (1, null),
                (_, state) => (state.ViolationCount + 1, state.BlockedUntil));

            var (currentViolations, currentBlockUntil) = currentState;

            // If user hit the violation limit, block them
            if (currentViolations >= MaxViolations)
            {
                _userViolations[userId] = (currentViolations, DateTime.UtcNow.Add(BlockDuration));
            }
        }
    }
}