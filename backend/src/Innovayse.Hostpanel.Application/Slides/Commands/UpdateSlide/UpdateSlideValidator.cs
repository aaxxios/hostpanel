using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Application.Slides.Common;
using Innovayse.Hostpanel.Application.Slides.Validators;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlide;

/// <summary>Validates <see cref="UpdateSlideCommand"/> before it reaches the handler.</summary>
public sealed class UpdateSlideValidator : LocalizedValidator<UpdateSlideCommand>
{
    /// <summary>Longest an icon name may be — matches the <c>slides.IconName</c> column.</summary>
    private const int IconNameMaxLength = 100;

    /// <summary>Longest a brand color may be — matches the <c>slides.BrandColor</c> column.</summary>
    private const int BrandColorMaxLength = 20;

    /// <summary>Longest an image URL may be — matches the <c>slides.ImageUrl</c> column.</summary>
    private const int ImageUrlMaxLength = 500;

    /// <summary>Longest a demo or learn-more link may be — matches their columns.</summary>
    private const int LinkMaxLength = 500;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateSlideValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).MustBePositiveId(L["SlideIdInvalid"]);
        RuleFor(x => x.IconName).NotEmpty().WithMessage(L["SlideIconNameRequired"])
            .MaximumLength(IconNameMaxLength).WithMessage(L["SlideIconNameTooLong", IconNameMaxLength]);
        RuleFor(x => x.BrandColor).NotEmpty().WithMessage(L["SlideBrandColorRequired"])
            .MaximumLength(BrandColorMaxLength).WithMessage(L["SlideBrandColorTooLong", BrandColorMaxLength]);
        RuleFor(x => x.ImageUrl).NotEmpty().WithMessage(L["SlideImageUrlRequired"])
            .MaximumLength(ImageUrlMaxLength).WithMessage(L["SlideImageUrlTooLong", ImageUrlMaxLength]);
        RuleFor(x => x.DemoUrl).MaximumLength(LinkMaxLength).When(x => x.DemoUrl is not null)
            .WithMessage(L["SlideDemoUrlTooLong", LinkMaxLength]);
        RuleFor(x => x.LearnMoreUrl).MaximumLength(LinkMaxLength).When(x => x.LearnMoreUrl is not null)
            .WithMessage(L["SlideLearnMoreUrlTooLong", LinkMaxLength]);
        RuleFor(x => x.ProductId).GreaterThan(0).When(x => x.ProductId is not null)
            .WithMessage(L["ProductIdInvalid"]);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).WithMessage(L["SlideSortOrderInvalid"]);
        RuleFor(x => x.TargetAudience).IsInEnum().WithMessage(L["SlideTargetAudienceInvalid"]);
        RuleForEach(x => x.Translations).SetValidator(new SlideTranslationValidator(localizer));
    }
}
