using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.DeleteDownloadCategory;

/// <summary>Validator for <see cref="DeleteDownloadCategoryCommand"/>.</summary>
public sealed class DeleteDownloadCategoryValidator : LocalizedValidator<DeleteDownloadCategoryCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public DeleteDownloadCategoryValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["DownloadCategoryIdInvalid"]);
    }
}
