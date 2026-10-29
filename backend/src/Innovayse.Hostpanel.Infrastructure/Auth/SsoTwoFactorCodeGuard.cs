using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// What a deployment whose people belong to an external sign-on provider gets for the per-account limits
/// on local one-time codes: a guard that admits no attempt and accepts no code.
/// </summary>
/// <remarks>
/// The sign-on provider checks its own codes, so no local code is ever checked in this mode; this exists so
/// the container is complete in every mode. Every refusal is the port's plain <c>false</c>, the answer a
/// locked account gets, so anything that did reach it fails closed.
/// </remarks>
public sealed class SsoTwoFactorCodeGuard : ITwoFactorCodeGuard
{
    /// <inheritdoc/>
    public bool TryBeginAttempt(string userId) => false;

    /// <inheritdoc/>
    /// <remarks>Nothing to give back: <see cref="TryBeginAttempt"/> never takes an attempt in this mode.</remarks>
    public void Forgive(string userId)
    {
        // Deliberately empty; see the remarks.
    }

    /// <inheritdoc/>
    public bool TryAcceptTimeStep(string userId, long timeStep) => false;
}
