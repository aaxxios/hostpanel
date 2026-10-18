namespace Innovayse.Hostpanel.Application.Resources;

/// <summary>
/// Marker type naming the <c>ErrorMessages*.resx</c> resource set: the sentence for each
/// <c>HostpanelErrorCodes</c> constant, keyed by the code. Injected as
/// <c>IStringLocalizer&lt;ErrorMessages&gt;</c>; never instantiated.
/// </summary>
/// <remarks>
/// The type's full name is the resource base name: it must keep matching the manifest name MSBuild
/// gives <c>Resources/ErrorMessages.resx</c>, or the localizer silently answers key names.
/// </remarks>
public sealed class ErrorMessages
{
    /// <summary>Prevents construction; the type exists only to name a resource set.</summary>
    private ErrorMessages()
    {
    }
}
