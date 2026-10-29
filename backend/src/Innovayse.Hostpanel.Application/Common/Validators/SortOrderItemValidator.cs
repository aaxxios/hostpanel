using FluentValidation;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Common.Validators;

/// <summary>
/// Validates one entry of a storefront content reorder request; shared by every content module's
/// reorder validator so the rule and its messages exist once.
/// </summary>
public sealed class SortOrderItemValidator : LocalizedValidator<SortOrderItem>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SortOrderItemValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(i => i.Id).MustBePositiveId(L["ContentIdInvalid"]);
        RuleFor(i => i.SortOrder).GreaterThanOrEqualTo(0).WithMessage(L["ContentSortOrderInvalid"]);
    }
}
