using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.InviteMyUser;

/// <summary>Validates <see cref="InviteMyUserCommand"/> before the handler executes.</summary>
public sealed class InviteMyUserValidator : LocalizedValidator<InviteMyUserCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public InviteMyUserValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256).WithMessage(L["ClientInviteEmailInvalid"]);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage(L["ClientInviteFirstNameInvalid"]);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage(L["ClientInviteLastNameInvalid"]);
    }
}
