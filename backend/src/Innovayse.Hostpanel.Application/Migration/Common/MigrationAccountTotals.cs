namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>How many client accounts and their sub-contacts the legacy source holds.</summary>
/// <param name="Clients">Client accounts available to pull.</param>
/// <param name="Contacts">Client sub-contacts available to pull.</param>
public sealed record MigrationAccountTotals(int Clients = 0, int Contacts = 0);
