using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.ApplyPaidRenewals;

/// <summary>Validates <see cref="ApplyPaidRenewalsCommand"/>.</summary>
public sealed class ApplyPaidRenewalsValidator : LocalizedValidator<ApplyPaidRenewalsCommand>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public ApplyPaidRenewalsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).GreaterThan(0).WithMessage(L["InvoiceIdInvalid"]);
    }
}
