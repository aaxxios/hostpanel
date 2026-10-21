using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Locations.Commands.UpdateLocationOrder;

/// <summary>Validates <see cref="UpdateLocationOrderCommand"/> before it reaches the handler.</summary>
public sealed class UpdateLocationOrderValidator : LocalizedValidator<UpdateLocationOrderCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateLocationOrderValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Items).NotNull().WithMessage(L["ContentOrderItemsRequired"]);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).MustBePositiveId(L["ContentIdInvalid"]);
            item.RuleFor(i => i.SortOrder).GreaterThanOrEqualTo(0).WithMessage(L["ContentSortOrderInvalid"]);
        });
    }
}
