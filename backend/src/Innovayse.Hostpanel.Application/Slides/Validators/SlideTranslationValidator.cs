using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Application.Slides.Common;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Slides.Validators;

/// <summary>Validates one <see cref="SlideTranslationDto"/>; shared by the create and update validators.</summary>
public sealed class SlideTranslationValidator : LocalizedValidator<SlideTranslationDto>
{
    /// <summary>Longest a locale code may be — matches the <c>slide_translations.Locale</c> column.</summary>
    private const int LocaleMaxLength = 10;

    /// <summary>Longest a title may be — matches the <c>slide_translations.Title</c> column.</summary>
    private const int TitleMaxLength = 200;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SlideTranslationValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Locale).NotEmpty().WithMessage(L["SlideTranslationLocaleRequired"])
            .MaximumLength(LocaleMaxLength).WithMessage(L["SlideTranslationLocaleTooLong", LocaleMaxLength]);
        RuleFor(x => x.Title).NotEmpty().WithMessage(L["SlideTranslationTitleRequired"])
            .MaximumLength(TitleMaxLength).WithMessage(L["SlideTranslationTitleTooLong", TitleMaxLength]);
    }
}
