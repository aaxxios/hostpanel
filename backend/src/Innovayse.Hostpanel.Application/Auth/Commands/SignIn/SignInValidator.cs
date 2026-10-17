using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.SignIn;

/// <summary>Validator for <see cref="SignInCommand"/>: both the email and the password must be present.</summary>
public sealed class SignInValidator : LocalizedValidator<SignInCommand>
{
    /// <summary>Initializes the validator.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SignInValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(L["SignInEmailRequired"]);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(L["SignInPasswordRequired"]);
    }
}
