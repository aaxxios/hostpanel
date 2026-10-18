using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateDownload;

/// <summary>Validator for <see cref="UpdateDownloadCommand"/>.</summary>
public sealed class UpdateDownloadValidator : LocalizedValidator<UpdateDownloadCommand>
{
    /// <summary>Longest accepted download title.</summary>
    private const int TitleMaxLength = 500;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateDownloadValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["DownloadIdInvalid"]);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(TitleMaxLength)
            .WithMessage(L["DownloadTitleRequired"]);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage(L["DownloadCategoryIdInvalid"]);
    }
}
