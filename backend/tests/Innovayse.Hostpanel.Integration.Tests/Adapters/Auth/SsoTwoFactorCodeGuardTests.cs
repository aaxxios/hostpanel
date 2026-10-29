using Innovayse.Hostpanel.Infrastructure.Auth;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Auth;

/// <summary>
/// The sign-on mode stand-in for the per-account limits on local one-time codes: it fails closed, answering
/// every attempt the way a locked account is answered.
/// </summary>
public sealed class SsoTwoFactorCodeGuardTests
{
    /// <summary>An account id a caller might pass.</summary>
    private const string UserId = "user-1";

    /// <summary>The adapter under test; it holds no state, so one instance serves every test.</summary>
    private readonly SsoTwoFactorCodeGuard _guard = new();

    /// <summary>No attempt is admitted, and forgiving one does not open the guard.</summary>
    [Fact]
    public void TryBeginAttempt_RefusesEvenAfterForgive()
    {
        _guard.TryBeginAttempt(UserId).Should().BeFalse();

        var forgive = () => _guard.Forgive(UserId);
        forgive.Should().NotThrow();
        _guard.TryBeginAttempt(UserId).Should().BeFalse();
    }

    /// <summary>No time step is ever accepted, the first one included.</summary>
    [Fact]
    public void TryAcceptTimeStep_Refuses()
    {
        _guard.TryAcceptTimeStep(UserId, 59_000_000).Should().BeFalse();
        _guard.TryAcceptTimeStep(UserId, 59_000_001).Should().BeFalse();
    }
}
