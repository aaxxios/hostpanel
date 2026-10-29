using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.Register;

/// <summary>
/// Validator for <see cref="RegisterCommand"/>: the email, the password and both names must be
/// present, so a body that omits one is a 400 naming that field. Whether the address is well formed
/// or the password meets the policy is the identity store's to judge, and a refusal there is answered
/// by the handler as <c>REGISTRATION_FAILED</c>.
/// </summary>
public sealed class RegisterValidator : LocalizedValidator<RegisterCommand>
{
    /// <summary>Initializes the validator.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RegisterValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage(L["RegisterEmailRequired"]);
        RuleFor(x => x.Password).NotEmpty().WithMessage(L["RegisterPasswordRequired"]);
        RuleFor(x => x.FirstName).NotEmpty().WithMessage(L["RegisterFirstNameRequired"]);
        RuleFor(x => x.LastName).NotEmpty().WithMessage(L["RegisterLastNameRequired"]);
    }
}
