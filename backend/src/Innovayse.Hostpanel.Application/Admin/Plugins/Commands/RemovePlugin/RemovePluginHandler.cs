using Innovayse.Hostpanel.Application.Admin.Plugins.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Admin.Plugins.Commands.RemovePlugin;

/// <summary>
/// Handles <see cref="RemovePluginCommand"/> -- deletes the plugin directory from disk.
/// </summary>
public sealed class RemovePluginHandler
{
    /// <summary>
    /// Deletes the plugin directory for the given plugin id.
    /// </summary>
    /// <param name="command">Command containing the plugin id to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A result requiring a restart when the plugin was removed; <c>PLUGIN_ID_INVALID</c> when the
    /// id is not a bare directory name; <c>PLUGIN_NOT_FOUND</c> when no plugin with that id is
    /// installed.
    /// </returns>
    public Task<Result<PluginActionResultDto>> HandleAsync(RemovePluginCommand command, CancellationToken ct)
    {
        var pluginsRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "plugins"));
        var pluginDir = Path.GetFullPath(Path.Combine(pluginsRoot, command.Id));

        // Guard against path traversal.
        if (!pluginDir.StartsWith(pluginsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(
                Result<PluginActionResultDto>.FailureCode(HostpanelErrorCodes.PLUGIN_ID_INVALID));
        }

        if (!Directory.Exists(pluginDir))
        {
            return Task.FromResult(
                Result<PluginActionResultDto>.FailureCode(HostpanelErrorCodes.PLUGIN_NOT_FOUND));
        }

        Directory.Delete(pluginDir, recursive: true);
        return Task.FromResult(Result<PluginActionResultDto>.Success(new PluginActionResultDto(RequiresRestart: true)));
    }
}
