using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// The one rule that keeps the published demo accounts usable: while demo mode is on, the sign-in
/// email and the password of the two configured demo accounts cannot change. Every flow that
/// writes either asks here first, so a visitor cannot lock everyone else out. With demo mode off
/// every check passes.
/// </summary>
public interface IDemoAccountGuard
{
    /// <summary>Checks a credential change for the account with the given subject.</summary>
    /// <param name="subject">The account's subject.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>DEMO_ACTION_DISABLED</c> when the account is a published demo account on a demo deployment.</returns>
    Task<Result> EnsureCredentialsUnchangedAsync(string subject, CancellationToken ct);

    /// <summary>Checks a credential change for the account with the given sign-in email.</summary>
    /// <param name="email">The account's current sign-in email.</param>
    /// <returns>Success, or <c>DEMO_ACTION_DISABLED</c> when the account is a published demo account on a demo deployment.</returns>
    Result EnsureCredentialsUnchanged(string email);
}
