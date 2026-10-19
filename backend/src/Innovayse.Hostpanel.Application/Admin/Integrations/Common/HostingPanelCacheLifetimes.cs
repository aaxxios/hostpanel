namespace Innovayse.Hostpanel.Application.Admin.Integrations.Common;

/// <summary>How long the hosting-panel server-info reads stay cached, shared by both panel generations.</summary>
public static class HostingPanelCacheLifetimes
{
    /// <summary>How long a successful server-info result is cached before being refreshed.</summary>
    public static readonly TimeSpan ServerInfo = TimeSpan.FromMinutes(5);
}
