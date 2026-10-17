using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateDownload;

/// <summary>Validator for <see cref="CreateDownloadCommand"/>.</summary>
public sealed class CreateDownloadValidator : LocalizedValidator<CreateDownloadCommand>
{
    /// <summary>Longest accepted download title.</summary>
    private const int TitleMaxLength = 500;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateDownloadValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(TitleMaxLength)
            .WithMessage(L["DownloadTitleRequired"]);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage(L["DownloadCategoryIdInvalid"]);
    }
}
