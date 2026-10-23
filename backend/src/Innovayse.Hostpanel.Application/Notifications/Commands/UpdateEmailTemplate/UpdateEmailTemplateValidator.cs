using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Notifications.Commands.UpdateEmailTemplate;

/// <summary>Validator for <see cref="UpdateEmailTemplateCommand"/>.</summary>
public sealed class UpdateEmailTemplateValidator : LocalizedValidator<UpdateEmailTemplateCommand>
{
    /// <summary>Longest accepted subject line, matching the <c>EmailTemplates</c> column.</summary>
    private const int SubjectMaxLength = 500;

    /// <summary>Longest accepted description, matching the <c>EmailTemplates</c> column.</summary>
    private const int DescriptionMaxLength = 500;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateEmailTemplateValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).MustBePositiveId(L["EmailTemplateIdInvalid"]);

        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(SubjectMaxLength)
            .WithMessage(L["EmailTemplateSubjectRequired"]);

        RuleFor(x => x.Body)
            .NotEmpty()
            .WithMessage(L["EmailTemplateBodyRequired"]);

        RuleFor(x => x.Description)
            .MaximumLength(DescriptionMaxLength)
            .When(x => x.Description is not null)
            .WithMessage(L["EmailTemplateDescriptionInvalid"]);
    }
}
