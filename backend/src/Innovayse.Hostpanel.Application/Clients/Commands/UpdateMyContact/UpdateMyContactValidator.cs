using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateMyContact;

/// <summary>
/// Validator for <see cref="UpdateMyContactCommand"/>. Declares no rules: the handler forwards the
/// fields to <c>UpdateContactCommand</c>, whose validator owns every field rule and which only runs
/// once the caller's profile has been found, so a caller with no profile still answers 404.
/// </summary>
public sealed class UpdateMyContactValidator : LocalizedValidator<UpdateMyContactCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateMyContactValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
