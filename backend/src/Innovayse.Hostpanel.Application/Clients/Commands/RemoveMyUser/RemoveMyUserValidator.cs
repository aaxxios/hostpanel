using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveMyUser;

/// <summary>Validates <see cref="RemoveMyUserCommand"/> before the handler executes.</summary>
public sealed class RemoveMyUserValidator : LocalizedValidator<RemoveMyUserCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RemoveMyUserValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.MemberUserId).NotEmpty().WithMessage(L["ClientUserIdRequired"]);
    }
}
