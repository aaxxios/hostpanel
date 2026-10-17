using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;

/// <summary>Validator for <see cref="SendEmailCommand"/>.</summary>
public sealed class SendEmailValidator : LocalizedValidator<SendEmailCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SendEmailValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.To).NotEmpty().EmailAddress().WithMessage(L["SendEmailToInvalid"]);

        RuleFor(x => x.TemplateSlug).NotEmpty().WithMessage(L["SendEmailTemplateSlugRequired"]);
    }
}
