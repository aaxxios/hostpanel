namespace Innovayse.Hostpanel.Domain.Servers.Enums;

/// <summary>
/// Identifies the control panel module used to manage a server.
/// </summary>
public enum ServerModule
{
    /// <summary>cPanel/WHM provisioning module.</summary>
    CPanel = 0,

    /// <summary>Plesk provisioning module.</summary>
    Plesk = 1,

    /// <summary>DirectAdmin provisioning module.</summary>
    DirectAdmin = 2,

    /// <summary>CentOS Web Panel provisioning module (legacy).</summary>
    Centos = 3,

    /// <summary>Control Web Panel 7 (CWP7) provisioning module.</summary>
    Cwp7 = 4,

    /// <summary>Lukittu license server module.</summary>
    Lukittu = 5,

    /// <summary>Virtualmin provisioning module.</summary>
    Virtualmin = 7,

    /// <summary>HyperVM VPS provisioning module.</summary>
    HyperVm = 8,
}
