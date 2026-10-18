using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.BulkInvoiceAction;

/// <summary>Validates <see cref="BulkInvoiceActionCommand"/>.</summary>
public sealed class BulkInvoiceActionValidator : LocalizedValidator<BulkInvoiceActionCommand>
{
    /// <summary>All valid bulk action names.</summary>
    private static readonly string[] ValidActions =
        ["MarkPaid", "MarkUnpaid", "MarkCancelled", "Duplicate", "Delete"];

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public BulkInvoiceActionValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceIds).NotEmpty().WithMessage(L["BulkInvoiceActionIdsRequired"]);
        RuleForEach(x => x.InvoiceIds).GreaterThan(0).WithMessage(L["BulkInvoiceActionIdInvalid"]);
        RuleFor(x => x.Action)
            .NotEmpty()
            .Must(a => ValidActions.Contains(a))
            .WithMessage(L["BulkInvoiceActionNameInvalid"]);
    }
}
