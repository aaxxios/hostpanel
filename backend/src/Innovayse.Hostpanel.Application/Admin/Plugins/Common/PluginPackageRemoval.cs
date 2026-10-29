namespace Innovayse.Hostpanel.Application.Admin.Plugins.Common;

/// <summary>The input of <see cref="Innovayse.Hostpanel.Application.Common.Interfaces.IPluginPackageStore.RemoveAsync"/>: the plugin to remove.</summary>
/// <param name="PluginId">The id of the uploaded plugin, as declared in its <c>plugin.json</c>.</param>
public sealed record PluginPackageRemoval(string PluginId);
