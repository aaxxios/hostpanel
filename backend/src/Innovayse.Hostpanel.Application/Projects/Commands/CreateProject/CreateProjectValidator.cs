using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Projects.Common;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Projects;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Projects.Commands.CreateProject;

/// <summary>Validates <see cref="CreateProjectCommand"/> before it reaches the handler.</summary>
public sealed class CreateProjectValidator : LocalizedValidator<CreateProjectCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateProjectValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Slug).NotEmpty().WithMessage(L["ProjectSlugRequired"])
            .MaximumLength(Project.SlugMaxLength).WithMessage(L["ProjectSlugTooLong", Project.SlugMaxLength])
            .Matches(Project.SlugPattern).WithMessage(L["ProjectSlugInvalid"]);
        RuleFor(x => x.Url).MaximumLength(Project.UrlMaxLength).WithMessage(L["ContentUrlTooLong", Project.UrlMaxLength])
            .MustBeWebLink(L["ContentUrlInvalid"]);
        RuleFor(x => x.ImageUrls).NotNull().WithMessage(L["ProjectImageUrlsRequired"])
            .Must(urls => urls is null || urls.Length <= Project.MaxImages)
            .WithMessage(L["ProjectImagesTooMany", Project.MaxImages]);
        RuleForEach(x => x.ImageUrls)
            .MaximumLength(Project.ImageUrlMaxLength).WithMessage(L["ContentUrlTooLong", Project.ImageUrlMaxLength])
            .MustBeWebLink(L["ContentUrlInvalid"]);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).WithMessage(L["ContentSortOrderInvalid"]);
        RuleFor(x => x.Translations).NotEmpty().WithMessage(L["ContentTranslationsRequired"]);
        RuleForEach(x => x.Translations).SetValidator(new ProjectTranslationValidator(localizer));
    }
}
