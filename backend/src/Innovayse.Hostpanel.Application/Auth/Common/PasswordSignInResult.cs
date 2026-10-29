using Innovayse.Hostpanel.Application.Auth.Common.Enums;

namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// The answer to one password sign-in attempt.
/// </summary>
/// <param name="Outcome">How the attempt ended.</param>
/// <param name="User">The account that signed in; set only when <paramref name="Outcome"/> is <see cref="PasswordSignInOutcome.Succeeded"/>.</param>
public sealed record PasswordSignInResult(PasswordSignInOutcome Outcome, UserIdentity? User);
