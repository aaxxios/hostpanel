namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerSshSettings;

/// <summary>Admin: reads a server's SSH key management settings.</summary>
/// <param name="ServerId">The server.</param>
public sealed record GetServerSshSettingsQuery(int ServerId);
