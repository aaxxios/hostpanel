using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.ProcessBillableItemsCron;

/// <summary>Validates <see cref="ProcessBillableItemsCronCommand"/>: no rules; the command carries no input.</summary>
public sealed class ProcessBillableItemsCronValidator : LocalizedValidator<ProcessBillableItemsCronCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ProcessBillableItemsCronValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
