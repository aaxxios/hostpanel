namespace Innovayse.Hostpanel.Shared.Resources;

/// <summary>
/// Marker type naming the shared <c>ValidationMessages*.resx</c> resource set: the titles and details
/// <c>ExceptionHandlingMiddleware</c> answers with and the model binders' messages. Injected as
/// <c>IStringLocalizer&lt;ValidationMessages&gt;</c>; never instantiated.
/// </summary>
/// <remarks>
/// The type's full name is the resource base name: it must keep matching the manifest name MSBuild
/// gives <c>Resources/ValidationMessages.resx</c>, or the localizer silently answers key names.
/// </remarks>
public sealed class ValidationMessages
{
    /// <summary>Prevents construction; the type exists only to name a resource set.</summary>
    private ValidationMessages()
    {
    }
}
