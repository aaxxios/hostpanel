using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreatePredefinedReplyCategory;

/// <summary>Validator for <see cref="CreatePredefinedReplyCategoryCommand"/>.</summary>
public sealed class CreatePredefinedReplyCategoryValidator : LocalizedValidator<CreatePredefinedReplyCategoryCommand>
{
    /// <summary>Longest accepted category name.</summary>
    private const int NameMaxLength = 255;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreatePredefinedReplyCategoryValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(NameMaxLength)
            .WithMessage(L["PredefinedReplyCategoryNameRequired"]);
    }
}
