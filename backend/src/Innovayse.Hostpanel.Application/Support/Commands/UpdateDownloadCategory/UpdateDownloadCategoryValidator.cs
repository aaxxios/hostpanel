using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateDownloadCategory;

/// <summary>Validator for <see cref="UpdateDownloadCategoryCommand"/>.</summary>
public sealed class UpdateDownloadCategoryValidator : LocalizedValidator<UpdateDownloadCategoryCommand>
{
    /// <summary>Longest accepted category name.</summary>
    private const int NameMaxLength = 255;

    /// <summary>Longest accepted category description.</summary>
    private const int DescriptionMaxLength = 1000;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateDownloadCategoryValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["DownloadCategoryIdInvalid"]);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(NameMaxLength)
            .WithMessage(L["DownloadCategoryNameRequired"]);

        RuleFor(x => x.Description)
            .MaximumLength(DescriptionMaxLength)
            .WithMessage(L["DownloadCategoryDescriptionInvalid"]);
    }
}
