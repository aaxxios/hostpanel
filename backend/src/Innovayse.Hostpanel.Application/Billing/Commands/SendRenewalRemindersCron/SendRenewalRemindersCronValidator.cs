using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SendRenewalRemindersCron;

/// <summary>Validates <see cref="SendRenewalRemindersCronCommand"/>: no rules; the command carries no input.</summary>
public sealed class SendRenewalRemindersCronValidator : LocalizedValidator<SendRenewalRemindersCronCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SendRenewalRemindersCronValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
