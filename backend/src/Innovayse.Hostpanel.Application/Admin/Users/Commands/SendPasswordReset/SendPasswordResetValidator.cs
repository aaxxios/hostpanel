using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.SendPasswordReset;

/// <summary>
///     Validates <see cref="SendPasswordResetCommand"/>. Its only field is the route-supplied
///     subject, so the single rule guards against an empty id; there is deliberately nothing
///     further to check.
/// </summary>
public sealed class SendPasswordResetValidator : LocalizedValidator<SendPasswordResetCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SendPasswordResetValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(L["UserIdRequired"]);
    }
}
