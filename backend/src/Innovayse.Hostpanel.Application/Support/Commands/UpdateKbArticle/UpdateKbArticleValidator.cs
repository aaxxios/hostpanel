using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateKbArticle;

/// <summary>Validator for <see cref="UpdateKbArticleCommand"/>.</summary>
public sealed class UpdateKbArticleValidator : LocalizedValidator<UpdateKbArticleCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateKbArticleValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).NotNull().GreaterThan(0).WithMessage(L["KbArticleIdInvalid"]);
        RuleFor(x => x.Title).NotEmpty().WithMessage(L["KbArticleTitleRequired"]);
        RuleFor(x => x.Content).NotEmpty().WithMessage(L["KbArticleContentRequired"]);
        RuleFor(x => x.Category).NotEmpty().WithMessage(L["KbArticleCategoryRequired"]);
    }
}
