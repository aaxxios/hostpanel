using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Admin.Plugins.Commands.InstallPlugin;

/// <summary>
/// Installs a plugin from a ZIP archive uploaded by the admin.
/// </summary>
/// <param name="Content">
/// Stream of the uploaded ZIP file, or <see langword="null"/> when the request carried no file; the
/// caller owns and disposes it.
/// </param>
public sealed record InstallPluginCommand(Stream? Content) : IDemoRestricted;
