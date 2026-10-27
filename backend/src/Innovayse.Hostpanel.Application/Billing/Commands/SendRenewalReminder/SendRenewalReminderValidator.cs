using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SendRenewalReminder;

/// <summary>Validates <see cref="SendRenewalReminderCommand"/>.</summary>
public sealed class SendRenewalReminderValidator : LocalizedValidator<SendRenewalReminderCommand>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public SendRenewalReminderValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).GreaterThan(0);
    }
}
