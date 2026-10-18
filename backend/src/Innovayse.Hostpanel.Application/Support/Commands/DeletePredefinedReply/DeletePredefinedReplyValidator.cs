using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.DeletePredefinedReply;

/// <summary>Validator for <see cref="DeletePredefinedReplyCommand"/>.</summary>
public sealed class DeletePredefinedReplyValidator : LocalizedValidator<DeletePredefinedReplyCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public DeletePredefinedReplyValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["PredefinedReplyIdInvalid"]);
    }
}
