using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateKbArticle;

/// <summary>Validator for <see cref="CreateKbArticleCommand"/>.</summary>
public sealed class CreateKbArticleValidator : LocalizedValidator<CreateKbArticleCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateKbArticleValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage(L["KbArticleTitleRequired"]);
        RuleFor(x => x.Content).NotEmpty().WithMessage(L["KbArticleContentRequired"]);
        RuleFor(x => x.Category).NotEmpty().WithMessage(L["KbArticleCategoryRequired"]);
    }
}
