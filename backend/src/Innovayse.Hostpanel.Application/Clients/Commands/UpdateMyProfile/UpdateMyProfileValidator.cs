using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateMyProfile;

/// <summary>
/// Validator for <see cref="UpdateMyProfileCommand"/>. Declares no rules: the handler forwards the
/// fields to <c>UpdateClientCommand</c>, whose validator owns every field rule and which only runs
/// once the caller's profile has been found -- so a caller with no profile still answers 404.
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
