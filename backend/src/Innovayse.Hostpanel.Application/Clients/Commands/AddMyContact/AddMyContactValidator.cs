using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.AddMyContact;

/// <summary>
/// Validator for <see cref="AddMyContactCommand"/>. Declares no rules: the handler forwards the
/// fields to <c>AddContactCommand</c>, whose validator owns every field rule and which only runs
/// once the caller's profile has been found -- so a caller with no profile still answers 404.
/// </summary>
public sealed class AddMyContactValidator : LocalizedValidator<AddMyContactCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AddMyContactValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
