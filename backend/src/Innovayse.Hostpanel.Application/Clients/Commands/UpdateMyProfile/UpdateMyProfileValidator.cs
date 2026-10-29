using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateMyProfile;

/// <summary>
/// Validator for <see cref="UpdateMyProfileCommand"/>. Declares no rules of its own: the handler
/// overlays the provided members on the stored profile and dispatches <c>UpdateClientCommand</c>,
/// whose validator owns every field rule, so each rule lives in one place and a caller with no
/// profile still answers 404 before any field is judged.
/// </summary>
public sealed class UpdateMyProfileValidator : LocalizedValidator<UpdateMyProfileCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateMyProfileValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
