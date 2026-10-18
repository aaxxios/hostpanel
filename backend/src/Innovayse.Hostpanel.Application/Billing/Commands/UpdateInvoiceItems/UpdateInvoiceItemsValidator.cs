using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceItems;

/// <summary>Validates <see cref="UpdateInvoiceItemsCommand"/> before it reaches the handler.</summary>
public sealed class UpdateInvoiceItemsValidator : LocalizedValidator<UpdateInvoiceItemsCommand>
{
    /// <summary>Maximum length of a line item description.</summary>
    private const int DescriptionMaxLength = 500;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateInvoiceItemsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).MustBePositiveId(L["InvoiceIdInvalid"]);
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
