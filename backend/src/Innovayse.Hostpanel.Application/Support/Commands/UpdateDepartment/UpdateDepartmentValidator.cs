using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateDepartment;

/// <summary>Validator for <see cref="UpdateDepartmentCommand"/>.</summary>
public sealed class UpdateDepartmentValidator : LocalizedValidator<UpdateDepartmentCommand>
{
    /// <summary>Longest accepted department name.</summary>
    private const int NameMaxLength = 255;

    /// <summary>Longest accepted department email address.</summary>
    private const int EmailMaxLength = 256;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateDepartmentValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["DepartmentIdInvalid"]);

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
