namespace Innovayse.Providers.CWP7.Models;

/// <summary>What CWP7 reports about one existing account.</summary>
/// <param name="User">The account username.</param>
/// <param name="Domain">The account's primary domain; empty when CWP7 did not send one.</param>
/// <param name="Suspended">Whether the account is currently suspended.</param>
internal sealed record Cwp7AccountDetail(string User, string Domain, bool Suspended);
