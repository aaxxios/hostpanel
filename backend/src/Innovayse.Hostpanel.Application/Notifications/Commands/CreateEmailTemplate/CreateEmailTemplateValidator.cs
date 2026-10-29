using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Notifications.Commands.CreateEmailTemplate;

/// <summary>Validator for <see cref="CreateEmailTemplateCommand"/>.</summary>
public sealed class CreateEmailTemplateValidator : LocalizedValidator<CreateEmailTemplateCommand>
{
    /// <summary>Longest accepted slug, matching the <c>EmailTemplates</c> column.</summary>
    private const int SlugMaxLength = 100;

    /// <summary>Longest accepted subject line, matching the <c>EmailTemplates</c> column.</summary>
    private const int SubjectMaxLength = 500;

    /// <summary>Longest accepted description, matching the <c>EmailTemplates</c> column.</summary>
    private const int DescriptionMaxLength = 500;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateEmailTemplateValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(SlugMaxLength)
            .WithMessage(L["EmailTemplateSlugRequired"]);

        RuleFor(x => x.Language)
            .Must(SupportedLanguages.IsSupported)
            .WithMessage(L["EmailTemplateLanguageInvalid"]);

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
