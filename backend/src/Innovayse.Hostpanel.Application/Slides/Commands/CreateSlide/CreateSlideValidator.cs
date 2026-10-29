using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Application.Slides.Common;
using Innovayse.Hostpanel.Domain.Slides;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Slides.Commands.CreateSlide;

/// <summary>Validates <see cref="CreateSlideCommand"/> before it reaches the handler.</summary>
public sealed class CreateSlideValidator : LocalizedValidator<CreateSlideCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateSlideValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.IconName).NotEmpty().WithMessage(L["SlideIconNameRequired"])
            .MaximumLength(Slide.IconNameMaxLength).WithMessage(L["SlideIconNameTooLong", Slide.IconNameMaxLength]);
        RuleFor(x => x.BrandColor).NotEmpty().WithMessage(L["SlideBrandColorRequired"])
            .MaximumLength(Slide.BrandColorMaxLength).WithMessage(L["SlideBrandColorTooLong", Slide.BrandColorMaxLength]);
        RuleFor(x => x.ImageUrl).NotEmpty().WithMessage(L["SlideImageUrlRequired"])
            .MaximumLength(Slide.ImageUrlMaxLength).WithMessage(L["SlideImageUrlTooLong", Slide.ImageUrlMaxLength])
            .MustBeWebLink(L["ContentUrlInvalid"]);
        RuleFor(x => x.DemoUrl).MaximumLength(Slide.LinkMaxLength).When(x => x.DemoUrl is not null)
            .WithMessage(L["SlideDemoUrlTooLong", Slide.LinkMaxLength])
            .MustBeWebLink(L["ContentUrlInvalid"]);
        RuleFor(x => x.LearnMoreUrl).MaximumLength(Slide.LinkMaxLength).When(x => x.LearnMoreUrl is not null)
            .WithMessage(L["SlideLearnMoreUrlTooLong", Slide.LinkMaxLength])
            .MustBeWebLink(L["ContentUrlInvalid"]);
        RuleFor(x => x.ProductId).GreaterThan(0).When(x => x.ProductId is not null)
            .WithMessage(L["ProductIdInvalid"]);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).WithMessage(L["SlideSortOrderInvalid"]);
        RuleFor(x => x.TargetAudience).IsInEnum().WithMessage(L["SlideTargetAudienceInvalid"]);
        RuleFor(x => x.Translations).NotEmpty().WithMessage(L["ContentTranslationsRequired"]);
        RuleFor(x => x.Translations).MustHaveDistinctLocales(t => t.Locale, L["ContentTranslationLocaleDuplicate"]);
        RuleFor(x => x.Translations).MustIncludeDefaultLocale(t => t.Locale, L["ContentDefaultTranslationRequired"]);
        RuleForEach(x => x.Translations).SetValidator(new SlideTranslationValidator(localizer));
    }
}
