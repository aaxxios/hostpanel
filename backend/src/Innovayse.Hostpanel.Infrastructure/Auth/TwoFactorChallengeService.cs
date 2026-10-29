using System.Security.Cryptography;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// Issues and redeems the pending token of a local two-factor sign-in with a purpose-bound,
/// time-limited Data Protection protector: the token is authenticated and encrypted with the
/// application key ring, so it cannot be forged, read or altered, and it expires on its own.
/// </summary>
/// <remarks>
/// Attempts and the single use are counted per challenge id in <see cref="IMemoryCache"/>, under one
/// lock so parallel guesses cannot overshoot the budget. The counter lives as long as the token can,
/// then the token itself is dead. This holds for one API instance, which is how the product runs; a
/// second instance would need a shared store for the counters (the token itself already works across
/// instances that share the key ring).
/// </remarks>
public sealed class TwoFactorChallengeService : ITwoFactorChallengeService
{
    /// <summary>The Data Protection purpose; a token minted for any other purpose cannot be read here.</summary>
    public const string Purpose = "Hostpanel.Auth.TwoFactorPending";

    /// <summary>Prefix of the cache key a challenge state is kept under.</summary>
    private const string CacheKeyPrefix = "TwoFactorChallenge:";

    /// <summary>Separates the challenge id from the user id inside the protected payload; the id is hexadecimal and never contains it.</summary>
    private const char PayloadSeparator = ':';

    /// <summary>Random bytes in a challenge id.</summary>
    private const int ChallengeIdBytes = 16;

    /// <summary>The time-limited protector for <see cref="Purpose"/>.</summary>
    private readonly ITimeLimitedDataProtector _protector;

    /// <summary>Where the attempts and the single use of each challenge are remembered.</summary>
    private readonly IMemoryCache _cache;

    /// <summary>Token lifetime and attempt budget.</summary>
    private readonly TwoFactorChallengeOptions _options;

    /// <summary>Guards every read-modify-write of a challenge state.</summary>
    private readonly object _gate = new();

    /// <summary>Initializes a new instance of the <see cref="TwoFactorChallengeService"/> class.</summary>
    /// <param name="dataProtection">The application Data Protection provider, backed by its key ring.</param>
    /// <param name="cache">Where the attempts and the single use of each challenge are remembered.</param>
    /// <param name="options">Token lifetime and attempt budget.</param>
    public TwoFactorChallengeService(
        IDataProtectionProvider dataProtection,
        IMemoryCache cache,
        IOptions<TwoFactorChallengeOptions> options)
    {
        _protector = dataProtection.CreateProtector(Purpose).ToTimeLimitedDataProtector();
        _cache = cache;
        _options = options.Value;
    }

    /// <summary>How long one challenge state is kept: as long as its token could still be presented.</summary>
    private TimeSpan Lifetime => TimeSpan.FromSeconds(_options.LifetimeSeconds);

    /// <inheritdoc/>
    public string Issue(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(ChallengeIdBytes));
        return _protector.Protect($"{id}{PayloadSeparator}{userId}", Lifetime);
    }

    /// <inheritdoc/>
    public TwoFactorChallengeDto? TryRead(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        string payload;
        try
        {
            // Throws for a forged, altered, foreign-purpose or expired token alike.
            payload = _protector.Unprotect(token, out _);
        }
        catch (CryptographicException)
        {
            return null;
        }

        var split = payload.IndexOf(PayloadSeparator, StringComparison.Ordinal);
        if (split <= 0 || split == payload.Length - 1)
        {
            return null;
        }

        var challenge = new TwoFactorChallengeDto(payload[..split], payload[(split + 1)..]);
        lock (_gate)
        {
            return IsOpen(StateOf(challenge)) ? challenge : null;
        }
    }

    /// <inheritdoc/>
    public bool TryBeginAttempt(TwoFactorChallengeDto challenge)
    {
        ArgumentNullException.ThrowIfNull(challenge);

        lock (_gate)
        {
            var state = StateOf(challenge);
            if (!IsOpen(state))
            {
                return false;
            }

            state.Attempts++;
            return true;
        }
    }

    /// <inheritdoc/>
    public void Complete(TwoFactorChallengeDto challenge)
    {
        ArgumentNullException.ThrowIfNull(challenge);

        lock (_gate)
        {
            StateOf(challenge).Spent = true;
        }
    }

    /// <summary>Whether a challenge may still take an attempt.</summary>
    /// <param name="state">The remembered state of the challenge.</param>
    /// <returns><c>true</c> when it is unspent and under the attempt budget.</returns>
    private bool IsOpen(ChallengeState state)
    {
        return !state.Spent && state.Attempts < _options.MaxAttempts;
    }

    /// <summary>Finds the state of a challenge, creating it on first sight. Call only under the lock.</summary>
    /// <param name="challenge">The challenge the state is tracked for.</param>
    /// <returns>The state, shared by every request that presents the same challenge.</returns>
    private ChallengeState StateOf(TwoFactorChallengeDto challenge)
    {
        return _cache.GetOrCreate(CacheKeyPrefix + challenge.Id, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = Lifetime;

            // Never evicted under memory pressure: a dropped state would reopen a spent challenge.
            entry.Priority = CacheItemPriority.NeverRemove;
            return new ChallengeState();
        })!;
    }
}
