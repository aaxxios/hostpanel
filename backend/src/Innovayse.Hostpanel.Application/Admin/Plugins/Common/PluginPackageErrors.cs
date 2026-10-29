using Innovayse.Hostpanel.Application.Admin.Plugins.Common.Enums;
using Innovayse.Hostpanel.Application.Common.Errors;

namespace Innovayse.Hostpanel.Application.Admin.Plugins.Common;

/// <summary>Maps a refused <see cref="PluginPackageOutcome"/> to the error code the API answers with.</summary>
public static class PluginPackageErrors
{
    /// <summary>The error code for a refused outcome.</summary>
    /// <param name="outcome">A refusal; the success outcomes have no code.</param>
    /// <returns>The matching <see cref="HostpanelErrorCodes"/> constant.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The outcome is a success.</exception>
    public static string CodeFor(PluginPackageOutcome outcome)
    {
        return outcome switch
        {
            PluginPackageOutcome.ManifestInvalid => HostpanelErrorCodes.PLUGIN_MANIFEST_INVALID,
            PluginPackageOutcome.SdkVersionUnsupported => HostpanelErrorCodes.PLUGIN_SDK_VERSION_UNSUPPORTED,
            PluginPackageOutcome.IdInvalid => HostpanelErrorCodes.PLUGIN_ID_INVALID,
            PluginPackageOutcome.ArchiveUnsafe => HostpanelErrorCodes.PLUGIN_ARCHIVE_UNSAFE,
            PluginPackageOutcome.ArchiveTooLarge => HostpanelErrorCodes.PLUGIN_ARCHIVE_TOO_LARGE,
            PluginPackageOutcome.HostAssemblyRefused => HostpanelErrorCodes.PLUGIN_HOST_ASSEMBLY_REFUSED,
            PluginPackageOutcome.NotTrusted => HostpanelErrorCodes.PLUGIN_NOT_TRUSTED,
            PluginPackageOutcome.BuiltIn => HostpanelErrorCodes.PLUGIN_BUILT_IN,
            PluginPackageOutcome.NotFound => HostpanelErrorCodes.PLUGIN_NOT_FOUND,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "A success outcome has no error code."),
        };
    }
}
