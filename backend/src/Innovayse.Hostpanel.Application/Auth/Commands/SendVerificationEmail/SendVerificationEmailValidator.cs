using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.SendVerificationEmail;

/// <summary>
/// Validates <see cref="SendVerificationEmailCommand"/>. Its only field is the account subject, so the
/// single rule guards against an empty one.
/// </summary>
public sealed class SendVerificationEmailValidator : LocalizedValidator<SendVerificationEmailCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SendVerificationEmailValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.AccountSubject).NotEmpty().WithMessage(L["UserIdRequired"]);
    }
}
