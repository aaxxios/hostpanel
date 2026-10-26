using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// Keeps, per account, the count of wrong one-time codes inside a fixed window and the newest
/// time step already accepted, in <see cref="IMemoryCache"/> under one lock.
/// </summary>
/// <remarks>
/// Like <see cref="TwoFactorChallengeService"/> this is correct for one API instance, which is how
/// the product runs; a second instance would need a shared store for these counters.
/// </remarks>
public sealed class TwoFactorCodeGuard : ITwoFactorCodeGuard
{
    /// <summary>Prefix of the cache key holding an account failure count.</summary>
    private const string FailuresKeyPrefix = "TwoFactorCodeGuard:Failures:";

    /// <summary>Prefix of the cache key holding an account newest accepted time step.</summary>
    private const string StepKeyPrefix = "TwoFactorCodeGuard:Step:";

    /// <summary>How long an accepted time step is remembered; far longer than any code stays valid.</summary>
    private static readonly TimeSpan StepMemory = TimeSpan.FromMinutes(10);

    /// <summary>Where the counters are kept.</summary>
    private readonly IMemoryCache _cache;

    /// <summary>The failure cap and its window.</summary>
    private readonly TwoFactorCodeGuardOptions _options;

    /// <summary>The clock that decides when a failure window has ended.</summary>
    private readonly TimeProvider _time;

    /// <summary>Guards every read-modify-write of a counter.</summary>
    private readonly object _gate = new();

    /// <summary>Initializes a new instance of the <see cref="TwoFactorCodeGuard"/> class.</summary>
    /// <param name="cache">Where the counters are kept.</param>
    /// <param name="options">The failure cap and its window.</param>
    /// <param name="time">The clock that decides when a failure window has ended.</param>
    public TwoFactorCodeGuard(IMemoryCache cache, IOptions<TwoFactorCodeGuardOptions> options, TimeProvider time)
    {
        _cache = cache;
        _options = options.Value;
        _time = time;
    }

    /// <inheritdoc/>
    public bool TryBeginAttempt(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        lock (_gate)
        {
            var failures = FailuresOf(userId);
            if (failures.Count >= _options.MaxFailures)
            {
                return false;
            }

            failures.Count++;
            return true;
        }
    }

    /// <inheritdoc/>
    public void Forgive(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        lock (_gate)
        {
            var failures = FailuresOf(userId);
            failures.Count = Math.Max(0, failures.Count - 1);
        }
    }

    /// <inheritdoc/>
    public bool TryAcceptTimeStep(string userId, long timeStep)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        lock (_gate)
        {
            var key = StepKeyPrefix + userId;
            if (_cache.TryGetValue(key, out long newest) && timeStep <= newest)
            {
                return false;
            }

            _cache.Set(key, timeStep, StepMemory);
            return true;
        }
    }

    /// <summary>Finds an account failure counter, starting a window on first sight. Call only under the lock.</summary>
    /// <param name="userId">The account.</param>
    /// <returns>The counter, shared by every request for the account.</returns>
    private FailureCount FailuresOf(string userId)
    {
        var key = FailuresKeyPrefix + userId;
        var now = _time.GetUtcNow();
        if (_cache.TryGetValue(key, out FailureCount? current) && current is not null && now < current.WindowEndsAt)
        {
            return current;
        }

        // The window is judged by the injected clock; the cache expiry below only frees the memory.
        var window = TimeSpan.FromMinutes(_options.WindowMinutes);
        var started = new FailureCount { WindowEndsAt = now + window };
        _cache.Set(key, started, window);
        return started;
    }

    /// <summary>The wrong codes counted for one account inside its window.</summary>
    private sealed class FailureCount
    {
        /// <summary>Gets or sets the number of attempts counted so far.</summary>
        public int Count { get; set; }

        /// <summary>Gets the instant the window closes; the next attempt after it starts a new count.</summary>
        public DateTimeOffset WindowEndsAt { get; init; }
    }
}
