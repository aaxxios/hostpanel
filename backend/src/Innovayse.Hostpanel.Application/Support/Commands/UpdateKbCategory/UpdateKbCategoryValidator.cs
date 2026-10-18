using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateKbCategory;

/// <summary>Validator for <see cref="UpdateKbCategoryCommand"/>.</summary>
public sealed class UpdateKbCategoryValidator : LocalizedValidator<UpdateKbCategoryCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateKbCategoryValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).NotNull().GreaterThan(0).WithMessage(L["KbCategoryIdInvalid"]);
        RuleFor(x => x.Name).NotEmpty().WithMessage(L["KbCategoryNameRequired"]);
        RuleFor(x => x.Description).NotEmpty().WithMessage(L["KbCategoryDescriptionRequired"]);
    }
}
