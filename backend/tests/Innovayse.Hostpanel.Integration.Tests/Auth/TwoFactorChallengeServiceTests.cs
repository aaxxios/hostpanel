using FluentAssertions;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// The pending-token service on its own: a token it issued reads back, anything else (forged,
/// altered, foreign-purpose, foreign-key-ring, expired) does not, and a challenge dies after its
/// attempt budget or its first success. No host or database is needed, only a key ring.
/// </summary>
public sealed class TwoFactorChallengeServiceTests
{
    /// <summary>The account the challenges are opened for.</summary>
    private const string UserId = "user-1";

    /// <summary>Builds a service over the given key ring.</summary>
    /// <param name="keyRing">The Data Protection provider whose keys sign and encrypt the tokens.</param>
    /// <param name="lifetimeSeconds">Token lifetime; negative means born expired.</param>
    /// <param name="maxAttempts">Attempts one challenge accepts.</param>
    /// <returns>The service under test.</returns>
    private static TwoFactorChallengeService Create(
        IDataProtectionProvider keyRing, int lifetimeSeconds = 300, int maxAttempts = 5)
    {
        var options = Options.Create(new TwoFactorChallengeOptions
        {
            LifetimeSeconds = lifetimeSeconds,
            MaxAttempts = maxAttempts,
        });
        return new TwoFactorChallengeService(keyRing, new MemoryCache(new MemoryCacheOptions()), options);
    }

    /// <summary>A token the service issued reads back as the account it was issued for.</summary>
    [Fact]
    public void IssuedToken_ReadsBackAsItsAccount()
    {
        var service = Create(new EphemeralDataProtectionProvider());

        var challenge = service.TryRead(service.Issue(UserId));

        challenge.Should().NotBeNull();
        challenge!.UserId.Should().Be(UserId);
    }

    /// <summary>Two tokens for the same account are different challenges.</summary>
    [Fact]
    public void TwoIssuedTokens_AreDistinctChallenges()
    {
        var service = Create(new EphemeralDataProtectionProvider());

        var first = service.Issue(UserId);
        var second = service.Issue(UserId);

        first.Should().NotBe(second);
        service.TryRead(first)!.Id.Should().NotBe(service.TryRead(second)!.Id);
    }

    /// <summary>Null, empty, plain text and the old base64 format are all refused.</summary>
    /// <param name="token">The presented token.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-token")]
    [InlineData("MmZhOnVzZXItMTo2Mzg1MDAwMDAwMDAwMDAwMDA=")]
    public void ForgedToken_IsRefused(string? token)
    {
        var service = Create(new EphemeralDataProtectionProvider());

        service.TryRead(token).Should().BeNull();
    }

    /// <summary>Changing one character of a genuine token breaks its authentication.</summary>
    [Fact]
    public void TamperedToken_IsRefused()
    {
        var service = Create(new EphemeralDataProtectionProvider());
        var token = service.Issue(UserId);
        var middle = token.Length / 2;
        var tampered = token[..middle] + (token[middle] == 'A' ? 'B' : 'A') + token[(middle + 1)..];

        service.TryRead(tampered).Should().BeNull();
    }

    /// <summary>A token protected for a different purpose under the same keys is not a pending token.</summary>
    [Fact]
    public void TokenOfAnotherPurpose_IsRefused()
    {
        var keyRing = new EphemeralDataProtectionProvider();
        var service = Create(keyRing);
        var foreign = keyRing.CreateProtector("Hostpanel.Something.Else")
            .ToTimeLimitedDataProtector()
            .Protect($"{new string('A', 32)}:{UserId}", TimeSpan.FromMinutes(5));

        service.TryRead(foreign).Should().BeNull();
    }

    /// <summary>A token minted under another key ring (another installation) is refused.</summary>
    [Fact]
    public void TokenFromAnotherKeyRing_IsRefused()
    {
        var other = Create(new EphemeralDataProtectionProvider());
        var service = Create(new EphemeralDataProtectionProvider());

        service.TryRead(other.Issue(UserId)).Should().BeNull();
    }

    /// <summary>A token past its lifetime is refused although its signature is genuine.</summary>
    [Fact]
    public void ExpiredToken_IsRefused()
    {
        var service = Create(new EphemeralDataProtectionProvider(), lifetimeSeconds: -60);

        service.TryRead(service.Issue(UserId)).Should().BeNull();
    }

    /// <summary>A challenge takes exactly its budget of attempts, then it is dead.</summary>
    [Fact]
    public void Challenge_DiesAfterItsAttemptBudget()
    {
        var service = Create(new EphemeralDataProtectionProvider(), maxAttempts: 3);
        var token = service.Issue(UserId);
        var challenge = service.TryRead(token)!;

        for (var attempt = 0; attempt < 3; attempt++)
        {
            service.TryBeginAttempt(challenge).Should().BeTrue();
        }

        service.TryBeginAttempt(challenge).Should().BeFalse();
        service.TryRead(token).Should().BeNull();
    }

    /// <summary>Concurrent guesses cannot take more attempts than the budget allows.</summary>
    [Fact]
    public void ParallelAttempts_NeverExceedTheBudget()
    {
        var service = Create(new EphemeralDataProtectionProvider(), maxAttempts: 5);
        var challenge = service.TryRead(service.Issue(UserId))!;
        var granted = 0;

        Parallel.For(0, 200, _ =>
        {
            if (service.TryBeginAttempt(challenge))
            {
                Interlocked.Increment(ref granted);
            }
        });

        granted.Should().Be(5);
    }

    /// <summary>A completed challenge cannot be read or attempted again.</summary>
    [Fact]
    public void CompletedChallenge_CannotBeUsedAgain()
    {
        var service = Create(new EphemeralDataProtectionProvider());
        var token = service.Issue(UserId);
        var challenge = service.TryRead(token)!;
        service.TryBeginAttempt(challenge).Should().BeTrue();

        service.Complete(challenge);

        service.TryRead(token).Should().BeNull();
        service.TryBeginAttempt(challenge).Should().BeFalse();
    }

    /// <summary>Spending or exhausting one challenge leaves another one open.</summary>
    [Fact]
    public void SpentChallenge_DoesNotAffectAnother()
    {
        var service = Create(new EphemeralDataProtectionProvider(), maxAttempts: 1);
        var spent = service.TryRead(service.Issue(UserId))!;
        var other = service.Issue(UserId);
        service.TryBeginAttempt(spent).Should().BeTrue();
        service.Complete(spent);

        service.TryRead(other).Should().NotBeNull();
    }
}
