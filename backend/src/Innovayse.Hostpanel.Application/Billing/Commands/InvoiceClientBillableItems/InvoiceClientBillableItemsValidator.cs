using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.InvoiceClientBillableItems;

/// <summary>Validates <see cref="InvoiceClientBillableItemsCommand"/>.</summary>
public sealed class InvoiceClientBillableItemsValidator : LocalizedValidator<InvoiceClientBillableItemsCommand>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public InvoiceClientBillableItemsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ClientIdInvalid"]);
        RuleFor(x => x.Type).IsInEnum().WithMessage(L["BillableItemTypeInvalid"]);
    }
}
