using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Slides;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Slides.Common;

/// <summary>Validates one <see cref="SlideTranslationDto"/>; shared by the create and update validators.</summary>
public sealed class SlideTranslationValidator : LocalizedValidator<SlideTranslationDto>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SlideTranslationValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Locale).NotEmpty().WithMessage(L["SlideTranslationLocaleRequired"])
            .Must(SupportedLanguages.IsSupported).WithMessage(L["ContentTranslationLocaleInvalid"]);
        RuleFor(x => x.Title).NotEmpty().WithMessage(L["SlideTranslationTitleRequired"])
            .MaximumLength(SlideTranslation.TitleMaxLength).WithMessage(L["SlideTranslationTitleTooLong", SlideTranslation.TitleMaxLength]);
        RuleFor(x => x.Features)
            .Must(features => features is null || features.Length <= SlideTranslation.FeaturesMaxCount)
            .WithMessage(L["SlideTranslationFeaturesTooMany", SlideTranslation.FeaturesMaxCount]);
        RuleForEach(x => x.Features)
            .MaximumLength(SlideTranslation.FeatureMaxLength)
            .WithMessage(L["SlideTranslationFeatureTooLong", SlideTranslation.FeatureMaxLength]);
        RuleFor(x => x.Tagline).MaximumLength(SlideTranslation.TaglineMaxLength)
            .WithMessage(L["SlideTranslationTaglineTooLong", SlideTranslation.TaglineMaxLength]);
        RuleFor(x => x.Description).MaximumLength(SlideTranslation.DescriptionMaxLength)
            .WithMessage(L["SlideTranslationDescriptionTooLong", SlideTranslation.DescriptionMaxLength]);
        RuleFor(x => x.CtaText).MaximumLength(SlideTranslation.CtaTextMaxLength)
            .WithMessage(L["SlideTranslationCtaTextTooLong", SlideTranslation.CtaTextMaxLength]);
        RuleFor(x => x.CtaUrl).MaximumLength(SlideTranslation.CtaUrlMaxLength)
            .WithMessage(L["ContentUrlTooLong", SlideTranslation.CtaUrlMaxLength])
            .MustBeWebLink(L["ContentUrlInvalid"]);
    }
}
