using FluentValidation;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Testimonials.Commands.UpdateTestimonialOrder;

/// <summary>Validates <see cref="UpdateTestimonialOrderCommand"/> before it reaches the handler.</summary>
public sealed class UpdateTestimonialOrderValidator : LocalizedValidator<UpdateTestimonialOrderCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateTestimonialOrderValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Items).NotNull().WithMessage(L["ContentOrderItemsRequired"]);
        RuleFor(x => x.Items).MustNotExceedSortOrderLimit(L["ContentOrderItemsTooMany", SortOrderLimits.MaxItems]);
        RuleForEach(x => x.Items).SetValidator(new SortOrderItemValidator(localizer));
    }
}
