using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlideOrder;

/// <summary>Validates <see cref="UpdateSlideOrderCommand"/> before it reaches the handler.</summary>
public sealed class UpdateSlideOrderValidator : LocalizedValidator<UpdateSlideOrderCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateSlideOrderValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).GreaterThan(0).WithMessage(L["SlideIdInvalid"]);
            item.RuleFor(i => i.SortOrder).GreaterThanOrEqualTo(0).WithMessage(L["SlideSortOrderInvalid"]);
        });
    }
}
