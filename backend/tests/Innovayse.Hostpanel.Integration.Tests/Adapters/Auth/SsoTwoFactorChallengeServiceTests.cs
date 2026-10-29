using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Infrastructure.Auth;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Auth;

/// <summary>
/// The sign-on mode stand-in for the local two-factor pending token: no challenge is ever open, so it
/// issues none and reads every token as dead.
/// </summary>
public sealed class SsoTwoFactorChallengeServiceTests
{
    /// <summary>The adapter under test; it holds no state, so one instance serves every test.</summary>
    private readonly SsoTwoFactorChallengeService _challenges = new();

    /// <summary>Issuing a challenge throws: no local password step can have passed in this mode.</summary>
    [Fact]
    public void Issue_Throws()
    {
        var act = () => _challenges.Issue("user-1");

        act.Should().Throw<InvalidOperationException>().WithMessage("Issue*");
    }

    /// <summary>Any token, present or not, reads as dead, the same silent answer a forged one gets.</summary>
    /// <param name="token">The token the client presented.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("CfDJ8-anything-a-client-could-send")]
    public void TryRead_ReturnsNull(string? token)
    {
        _challenges.TryRead(token).Should().BeNull();
    }

    /// <summary>No attempt is admitted, and completing a challenge changes nothing.</summary>
    [Fact]
    public void TryBeginAttempt_RefusesAndCompleteIsHarmless()
    {
        var challenge = new TwoFactorChallengeDto("challenge-1", "user-1");

        _challenges.TryBeginAttempt(challenge).Should().BeFalse();

        var complete = () => _challenges.Complete(challenge);
        complete.Should().NotThrow();
        _challenges.TryBeginAttempt(challenge).Should().BeFalse();
    }
}
