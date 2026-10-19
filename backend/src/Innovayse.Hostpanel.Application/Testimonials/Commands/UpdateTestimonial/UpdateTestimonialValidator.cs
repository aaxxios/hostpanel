using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Application.Testimonials.Common;
using Innovayse.Hostpanel.Domain.Testimonials;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Testimonials.Commands.UpdateTestimonial;

/// <summary>Validates <see cref="UpdateTestimonialCommand"/> before it reaches the handler.</summary>
public sealed class UpdateTestimonialValidator : LocalizedValidator<UpdateTestimonialCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateTestimonialValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).MustBePositiveId(L["ContentIdInvalid"]);
        RuleFor(x => x.Name).NotEmpty().WithMessage(L["TestimonialNameRequired"])
            .MaximumLength(Testimonial.NameMaxLength).WithMessage(L["TestimonialNameTooLong", Testimonial.NameMaxLength]);
        RuleFor(x => x.Company).MaximumLength(Testimonial.CompanyMaxLength).WithMessage(L["TestimonialCompanyTooLong", Testimonial.CompanyMaxLength]);
        RuleFor(x => x.AvatarUrl).MaximumLength(Testimonial.AvatarUrlMaxLength).WithMessage(L["ContentUrlTooLong", Testimonial.AvatarUrlMaxLength])
            .MustBeWebLink(L["ContentUrlInvalid"]);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage(L["TestimonialRatingInvalid"]);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).WithMessage(L["ContentSortOrderInvalid"]);
        RuleFor(x => x.Translations).NotEmpty().WithMessage(L["ContentTranslationsRequired"]);
        RuleForEach(x => x.Translations).SetValidator(new TestimonialTranslationValidator(localizer));
    }
}
