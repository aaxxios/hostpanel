using Innovayse.Hostpanel.Application.Admin.Plugins.Common;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// The plugin folders on disk: the plugins shipped with the build (read-only) and the folder
/// uploaded plugins are installed into. Implemented in Infrastructure; every file system access for
/// plugins goes through it.
/// </summary>
public interface IPluginPackageStore
{
    /// <summary>
    /// Checks an uploaded archive and, when it passes, unpacks it into its own folder. Nothing is
    /// left on disk when it is refused.
    /// </summary>
    /// <param name="upload">The uploaded archive.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The outcome; <see cref="Admin.Plugins.Common.Enums.PluginPackageOutcome.Installed"/> on success.</returns>
    Task<PluginPackageResult> InstallAsync(PluginPackageUpload upload, CancellationToken ct);

    /// <summary>Deletes an uploaded plugin's folder. A plugin shipped with the build is refused.</summary>
    /// <param name="removal">The plugin to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The outcome; <see cref="Admin.Plugins.Common.Enums.PluginPackageOutcome.Removed"/> on success.</returns>
    Task<PluginPackageResult> RemoveAsync(PluginPackageRemoval removal, CancellationToken ct);

    /// <summary>Every plugin on disk with a readable manifest, built-in ones first.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The plugins found.</returns>
    Task<IReadOnlyList<InstalledPluginPackage>> ListAsync(CancellationToken ct);
}
