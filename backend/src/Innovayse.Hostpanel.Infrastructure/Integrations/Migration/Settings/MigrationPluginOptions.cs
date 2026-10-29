namespace Innovayse.Hostpanel.Infrastructure.Integrations.Migration.Settings;

/// <summary>
/// Where the migration plugin for the source billing system is read from when an admin downloads
/// it. Bound from the <c>MigrationPlugin</c> section (<c>MigrationPlugin__Path</c> in compose).
/// </summary>
/// <remarks>
/// The API project copies the plugin's installable <c>modules</c> folder into its build and publish
/// output under <see cref="DefaultFolder"/>, so the default finds it on a developer machine, in the
/// tests and in the published image alike. Set <see cref="Path"/> only to serve the plugin from
/// somewhere else, such as a mounted volume.
/// </remarks>
public sealed class MigrationPluginOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "MigrationPlugin";

    /// <summary>The folder, under the application folder, the build copies the plugin into.</summary>
    public const string DefaultFolder = "migration-plugin";

    /// <summary>The folder inside <see cref="Root"/> that is installed into the source system and zipped.</summary>
    public const string InstallableFolder = "modules";

    /// <summary>
    /// The folder holding the plugin's installable <c>modules</c> folder, absolute or relative to the
    /// application folder. Must not be blank.
    /// </summary>
    public string Path { get; init; } = DefaultFolder;

    /// <summary>The resolved, absolute folder holding the plugin's installable <c>modules</c> folder.</summary>
    public string Root => System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, Path));

    /// <summary>The resolved, absolute installable folder the archive is built from.</summary>
    public string InstallableRoot => System.IO.Path.Combine(Root, InstallableFolder);
}
