using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.DeletePredefinedReplyCategory;

/// <summary>Validator for <see cref="DeletePredefinedReplyCategoryCommand"/>.</summary>
public sealed class DeletePredefinedReplyCategoryValidator : LocalizedValidator<DeletePredefinedReplyCategoryCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public DeletePredefinedReplyCategoryValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["PredefinedReplyCategoryIdInvalid"]);
    }
}
