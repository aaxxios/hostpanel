using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Faqs.Common;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Faqs.Commands.CreateFaq;

/// <summary>Validates <see cref="CreateFaqCommand"/> before it reaches the handler.</summary>
public sealed class CreateFaqValidator : LocalizedValidator<CreateFaqCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateFaqValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Category).IsInEnum().WithMessage(L["FaqCategoryInvalid"]);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).WithMessage(L["ContentSortOrderInvalid"]);
        RuleFor(x => x.Translations).NotEmpty().WithMessage(L["ContentTranslationsRequired"]);
        RuleForEach(x => x.Translations).SetValidator(new FaqTranslationValidator(localizer));
    }
}
