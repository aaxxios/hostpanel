namespace Innovayse.Hostpanel.Application.Admin.Plugins.Common;

/// <summary>The input of <see cref="Innovayse.Hostpanel.Application.Common.Interfaces.IPluginPackageStore.InstallAsync"/>: one uploaded plugin archive.</summary>
/// <param name="Content">The uploaded ZIP archive; the caller owns and disposes it. Read once, front to back.</param>
public sealed record PluginPackageUpload(Stream Content);
