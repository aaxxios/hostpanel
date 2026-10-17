using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateDownloadCategory;

/// <summary>Validator for <see cref="CreateDownloadCategoryCommand"/>.</summary>
public sealed class CreateDownloadCategoryValidator : LocalizedValidator<CreateDownloadCategoryCommand>
{
    /// <summary>Longest accepted category name.</summary>
    private const int NameMaxLength = 255;

    /// <summary>Longest accepted category description.</summary>
    private const int DescriptionMaxLength = 1000;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateDownloadCategoryValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(NameMaxLength)
            .WithMessage(L["DownloadCategoryNameRequired"]);

        RuleFor(x => x.Description)
            .MaximumLength(DescriptionMaxLength)
            .WithMessage(L["DownloadCategoryDescriptionInvalid"]);
    }
}
