using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateDepartment;

/// <summary>Validator for <see cref="CreateDepartmentCommand"/>.</summary>
public sealed class CreateDepartmentValidator : LocalizedValidator<CreateDepartmentCommand>
{
    /// <summary>Longest accepted department name.</summary>
    private const int NameMaxLength = 255;

    /// <summary>Longest accepted department email address.</summary>
    private const int EmailMaxLength = 256;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateDepartmentValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(NameMaxLength)
            .WithMessage(L["DepartmentNameRequired"]);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(EmailMaxLength)
            .WithMessage(L["DepartmentEmailInvalid"]);
    }
}
