using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.UpdateUser;

/// <summary>Validates <see cref="UpdateUserCommand"/> before it reaches the handler.</summary>
public sealed class UpdateUserValidator : LocalizedValidator<UpdateUserCommand>
{
    /// <summary>Longest a first or last name may be.</summary>
    private const int MaxNameLength = 100;

    /// <summary>Longest an email address may be.</summary>
    private const int MaxEmailLength = 256;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateUserValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(L["UserIdRequired"]);
        RuleFor(x => x.FirstName)
            .NotEmpty().MaximumLength(MaxNameLength).WithMessage(L["UserFirstNameInvalid"]);
        RuleFor(x => x.LastName)
            .NotEmpty().MaximumLength(MaxNameLength).WithMessage(L["UserLastNameInvalid"]);
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress().MaximumLength(MaxEmailLength).WithMessage(L["UserEmailInvalid"]);
    }
}
