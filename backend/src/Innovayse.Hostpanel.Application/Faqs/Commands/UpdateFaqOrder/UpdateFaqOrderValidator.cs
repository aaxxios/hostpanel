using FluentValidation;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Faqs.Commands.UpdateFaqOrder;

/// <summary>Validates <see cref="UpdateFaqOrderCommand"/> before it reaches the handler.</summary>
public sealed class UpdateFaqOrderValidator : LocalizedValidator<UpdateFaqOrderCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateFaqOrderValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Items).NotNull().WithMessage(L["ContentOrderItemsRequired"]);
        RuleFor(x => x.Items).MustNotExceedSortOrderLimit(L["ContentOrderItemsTooMany", SortOrderLimits.MaxItems]);
        RuleForEach(x => x.Items).SetValidator(new SortOrderItemValidator(localizer));
    }
}
