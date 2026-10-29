using Innovayse.Providers.CWP.Models.Enums;

namespace Innovayse.Providers.CWP.Models;

/// <summary>The typed answer to an account change.</summary>
/// <param name="Outcome">How CWP answered.</param>
/// <param name="ExistingDomain">
/// The primary domain of the account that already exists, when <paramref name="Outcome"/> is
/// <see cref="CwpAccountOutcome.AlreadyExists"/>; null otherwise.
/// </param>
public sealed record CwpAccountResult(CwpAccountOutcome Outcome, string? ExistingDomain);
