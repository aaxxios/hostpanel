using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// What a deployment whose people belong to an external sign-on provider gets for the pending token of a
/// local two-factor sign-in: a challenge service that issues nothing and accepts nothing. No key ring, no
/// cache, no Identity.
/// </summary>
/// <remarks>
/// It exists so the local sign-in and two-factor sign-in handlers can have their code generated in this
/// mode too; their routes answer 404 here (<c>LocalModeOnlyFilter</c>). The sign-on provider runs its own
/// second factor, so no local challenge is ever open: every token reads as dead, which the port defines as
/// the same silent <c>null</c>/<c>false</c> a forged or expired one gets, and issuing one throws through
/// <see cref="SsoModeRefusal"/> because no local password step can have passed.
/// </remarks>
public sealed class SsoTwoFactorChallengeService : ITwoFactorChallengeService
{
    /// <inheritdoc/>
    public string Issue(string userId) => throw SsoModeRefusal.LocalAccountsOnly(nameof(Issue));

    /// <inheritdoc/>
    public TwoFactorChallengeDto? TryRead(string? token) => null;

    /// <inheritdoc/>
    public bool TryBeginAttempt(TwoFactorChallengeDto challenge) => false;

    /// <inheritdoc/>
    /// <remarks>Nothing to spend: no challenge is ever open in this mode, so none can be completed.</remarks>
    public void Complete(TwoFactorChallengeDto challenge)
    {
        // Deliberately empty; see the remarks.
    }
}
