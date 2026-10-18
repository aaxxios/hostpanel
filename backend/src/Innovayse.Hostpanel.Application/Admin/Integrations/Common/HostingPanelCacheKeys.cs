namespace Innovayse.Hostpanel.Application.Admin.Integrations.Common;

/// <summary>
/// Memory-cache keys of the live hosting-panel server-info results. The read handlers store
/// under them and the save and test handlers evict them, so the literals live here once.
/// </summary>
public static class HostingPanelCacheKeys
{
    /// <summary>Key of the cached CentOS Web Panel server-info result.</summary>
    public const string Cwp = "cwp:server-info";

    /// <summary>Key of the cached CentOS Web Panel 7 server-info result.</summary>
    public const string Cwp7 = "cwp7:server-info";
}
