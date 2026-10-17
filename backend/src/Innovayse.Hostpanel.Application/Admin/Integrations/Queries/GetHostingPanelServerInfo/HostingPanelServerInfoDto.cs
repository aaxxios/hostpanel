namespace Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetHostingPanelServerInfo;

/// <summary>Live server status returned by the hosting panel server-info endpoint.</summary>
/// <param name="Connected">True if the API connection succeeded.</param>
/// <param name="AccountsCount">Total hosting accounts on the server, or null if unavailable.</param>
/// <param name="CwpVersion">Hosting panel software version string, or null if unavailable.</param>
/// <param name="LastTestedAt">UTC timestamp of the last successful connection test, or null.</param>
/// <param name="ErrorMessage">Human-readable error if Connected is false, otherwise null.</param>
public record HostingPanelServerInfoDto(
    bool Connected,
    int? AccountsCount,
    string? CwpVersion,
    DateTimeOffset? LastTestedAt,
    string? ErrorMessage);
