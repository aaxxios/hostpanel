using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdatePredefinedReply;

/// <summary>Validator for <see cref="UpdatePredefinedReplyCommand"/>.</summary>
public sealed class UpdatePredefinedReplyValidator : LocalizedValidator<UpdatePredefinedReplyCommand>
{
    /// <summary>Longest accepted reply name.</summary>
    private const int NameMaxLength = 255;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdatePredefinedReplyValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["PredefinedReplyIdInvalid"]);

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
