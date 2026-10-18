using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateKbCategory;

/// <summary>Validator for <see cref="CreateKbCategoryCommand"/>.</summary>
public sealed class CreateKbCategoryValidator : LocalizedValidator<CreateKbCategoryCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateKbCategoryValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(L["KbCategoryNameRequired"]);
        RuleFor(x => x.Description).NotEmpty().WithMessage(L["KbCategoryDescriptionRequired"]);
        RuleFor(x => x.ParentCategoryId)
            .GreaterThan(0)
            .When(x => x.ParentCategoryId.HasValue)
            .WithMessage(L["KbCategoryParentIdInvalid"]);
    }
}
