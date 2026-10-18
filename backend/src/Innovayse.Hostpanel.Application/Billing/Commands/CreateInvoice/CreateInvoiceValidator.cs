using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateInvoice;

/// <summary>Validates <see cref="CreateInvoiceCommand"/> before it reaches the handler.</summary>
public sealed class CreateInvoiceValidator : LocalizedValidator<CreateInvoiceCommand>
{
    /// <summary>Maximum length of a line item description.</summary>
    private const int DescriptionMaxLength = 500;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateInvoiceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["InvoiceClientIdInvalid"]);

        // Compared by date, not by instant. The rule used to read `d > DateTimeOffset.UtcNow`,
        // and the admin's date picker sends a date-only value that binds to midnight UTC -- so
        // "due today", an ordinary thing to raise an invoice for, was always already in the past
        // and would have been refused the moment this validator started running.
        RuleFor(x => x.DueDate)
            .Must(d => d.UtcDateTime.Date >= DateTime.UtcNow.Date)
            .WithMessage(L["InvoiceDueDateInPast"]);
        RuleFor(x => x.Items).NotEmpty().WithMessage(L["InvoiceItemsRequired"]);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description)
                .NotEmpty()
                .MaximumLength(DescriptionMaxLength)
                .WithMessage(L["InvoiceItemDescriptionInvalid", DescriptionMaxLength]);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0).WithMessage(L["InvoiceItemUnitPriceInvalid"]);
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage(L["InvoiceItemQuantityInvalid"]);
        });
    }
}
