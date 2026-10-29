namespace Innovayse.Hostpanel.Infrastructure.Security.Settings;

/// <summary>
/// The secrets a deployed host must have chosen for itself, gathered from their own configuration keys
/// so <see cref="DeployedSecretsValidator"/> can refuse a start that runs on a value copied from
/// a template. It is not bound from a section: <c>AddInfrastructure</c> fills it from the keys the
/// secrets already live under, so nothing new has to be configured.
/// </summary>
public sealed class DeployedSecrets
{
    /// <summary>The token signing secret (<c>Jwt__Secret</c>); the developer fallback when the key is unset.</summary>
    public string? JwtSecret { get; set; }

    /// <summary>The base64 data-encryption key (<c>EncryptionKey</c>).</summary>
    public string? EncryptionKey { get; set; }

    /// <summary>The key that authenticates the sign-out fan-out (<c>BackchannelLogout__Key</c>); empty when none is used.</summary>
    public string? BackchannelLogoutKey { get; set; }

    /// <summary>The database password taken from <c>ConnectionStrings__DefaultConnection</c>; empty when the connection uses none.</summary>
    public string? DatabasePassword { get; set; }
}
