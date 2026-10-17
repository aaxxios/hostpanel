using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.PublishKbArticle;

/// <summary>Validator for <see cref="PublishKbArticleCommand"/>.</summary>
public sealed class PublishKbArticleValidator : LocalizedValidator<PublishKbArticleCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public PublishKbArticleValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).NotNull().GreaterThan(0).WithMessage(L["KbArticleIdInvalid"]);
    }
}
