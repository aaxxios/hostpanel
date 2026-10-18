using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.CreateClient;

/// <summary>Validates <see cref="CreateClientCommand"/> before the handler executes.</summary>
public sealed class CreateClientValidator : LocalizedValidator<CreateClientCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateClientValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.SubjectUserId).NotEmpty().WithMessage(L["ClientUserIdRequired"]);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256).WithMessage(L["ClientNewUserEmailInvalid"]);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage(L["ClientFirstNameInvalid"]);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage(L["ClientLastNameInvalid"]);
        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage(L["ClientCompanyNameInvalid"])
            .When(x => x.CompanyName is not null);
    }
}
