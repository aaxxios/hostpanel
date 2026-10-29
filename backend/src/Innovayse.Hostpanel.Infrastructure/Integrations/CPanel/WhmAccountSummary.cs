namespace Innovayse.Hostpanel.Infrastructure.Integrations.CPanel;

/// <summary>What WHM's <c>accountsummary</c> reports about one account.</summary>
/// <param name="Username">The account's username.</param>
/// <param name="Domain">The account's primary domain; empty when WHM did not send one.</param>
/// <param name="Suspended">Whether the account is currently suspended.</param>
public sealed record WhmAccountSummary(string Username, string Domain, bool Suspended);
