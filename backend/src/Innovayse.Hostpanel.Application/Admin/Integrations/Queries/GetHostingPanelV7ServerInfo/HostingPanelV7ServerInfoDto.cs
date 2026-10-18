namespace Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetHostingPanelV7ServerInfo;

/// <summary>Live server status returned by the hosting panel (v7) server-info endpoint.</summary>
/// <param name="Connected">True if the API connection succeeded.</param>
/// <param name="PackagesCount">Total hosting packages on the server, or null if unavailable.</param>
/// <param name="LastTestedAt">UTC timestamp of the last successful connection test, or null.</param>
/// <param name="ErrorMessage">Human-readable error if Connected is false, otherwise null.</param>
public record HostingPanelV7ServerInfoDto(
    bool Connected,
    int? PackagesCount,
    DateTimeOffset? LastTestedAt,
    string? ErrorMessage);
