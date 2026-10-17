using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.InvoiceSelectedItems;

/// <summary>Validates <see cref="InvoiceSelectedItemsCommand"/> before it reaches the handler.</summary>
public sealed class InvoiceSelectedItemsValidator : LocalizedValidator<InvoiceSelectedItemsCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public InvoiceSelectedItemsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["BillableItemClientIdInvalid"]);
        RuleFor(x => x.BillableItemIds).NotEmpty().WithMessage(L["InvoiceSelectedItemsRequired"]);
    }
}
