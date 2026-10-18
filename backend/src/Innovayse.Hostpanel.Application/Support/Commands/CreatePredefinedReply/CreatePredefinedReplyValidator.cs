using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreatePredefinedReply;

/// <summary>Validator for <see cref="CreatePredefinedReplyCommand"/>.</summary>
public sealed class CreatePredefinedReplyValidator : LocalizedValidator<CreatePredefinedReplyCommand>
{
    /// <summary>Longest accepted reply name.</summary>
    private const int NameMaxLength = 255;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreatePredefinedReplyValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(NameMaxLength)
            .WithMessage(L["PredefinedReplyNameRequired"]);

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage(L["PredefinedReplyContentRequired"]);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage(L["PredefinedReplyCategoryIdInvalid"]);
    }
}
