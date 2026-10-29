using FluentValidation;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Partners.Commands.UpdatePartnerOrder;

/// <summary>Validates <see cref="UpdatePartnerOrderCommand"/> before it reaches the handler.</summary>
public sealed class UpdatePartnerOrderValidator : LocalizedValidator<UpdatePartnerOrderCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdatePartnerOrderValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Items).NotNull().WithMessage(L["ContentOrderItemsRequired"]);
        RuleFor(x => x.Items).MustNotExceedSortOrderLimit(L["ContentOrderItemsTooMany", SortOrderLimits.MaxItems]);
        RuleForEach(x => x.Items).SetValidator(new SortOrderItemValidator(localizer));
    }
}
