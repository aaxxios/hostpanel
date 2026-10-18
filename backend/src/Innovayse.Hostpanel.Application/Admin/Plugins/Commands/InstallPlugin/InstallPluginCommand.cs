using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Admin.Plugins.Commands.InstallPlugin;

/// <summary>
/// Installs a plugin from a ZIP archive uploaded by the admin.
/// </summary>
/// <param name="Content">Stream of the uploaded ZIP file; the caller owns and disposes it.</param>
/// <param name="Length">The declared length of the upload, in bytes.</param>
public record InstallPluginCommand(Stream Content, long Length) : IDemoRestricted;
