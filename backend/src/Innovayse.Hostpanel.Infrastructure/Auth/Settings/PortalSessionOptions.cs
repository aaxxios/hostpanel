namespace Innovayse.Hostpanel.Infrastructure.Auth.Settings;

/// <summary>
/// Where the client portal keeps its sign-in sessions, so a back-channel logout can end them. Bound
/// from the <c>PortalSessions</c> section (<c>PortalSessions__RedisConnection</c> in compose, fed the
/// same <c>REDIS_CONNECTION</c> the portal reads as <c>NUXT_SESSION_REDIS</c>).
/// </summary>
public sealed class PortalSessionOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "PortalSessions";

    /// <summary>
    /// The Redis connection string; empty means the portal keeps its sessions in its own process
    /// memory and there is nothing here to revoke.
    /// </summary>
    public string RedisConnection { get; init; } = string.Empty;
}
