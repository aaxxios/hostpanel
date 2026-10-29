namespace Innovayse.Hostpanel.Infrastructure.Integrations.Migration.Settings;

/// <summary>
/// How far the migration source client trusts the address an admin typed into a job. Bound from
/// the <c>MigrationSource</c> section (<c>MigrationSource__AllowInsecureHttp</c> and so on in compose).
/// </summary>
/// <remarks>
/// The source URL is admin input that this server then calls, so by default it must be HTTPS and
/// must not resolve to a loopback, link-local, private or otherwise internal address. Both
/// relaxations exist for local development, where the legacy install runs beside this one.
/// </remarks>
public sealed class MigrationSourceOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "MigrationSource";

    /// <summary>Whether a plain <c>http://</c> source URL is accepted. Off by default; for local development only.</summary>
    public bool AllowInsecureHttp { get; init; }

    /// <summary>Whether a source resolving to a loopback, link-local or private address is accepted. Off by default; for local development only.</summary>
    public bool AllowPrivateNetworks { get; init; }

    /// <summary>The largest response body read from the source, in bytes. A page of records larger than this fails the pull.</summary>
    public long MaxResponseBytes { get; init; } = 50L * 1024 * 1024;
}
