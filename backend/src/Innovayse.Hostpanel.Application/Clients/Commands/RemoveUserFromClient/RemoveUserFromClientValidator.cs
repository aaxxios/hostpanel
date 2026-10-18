using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveUserFromClient;

/// <summary>Validates <see cref="RemoveUserFromClientCommand"/> before the handler executes.</summary>
public sealed class RemoveUserFromClientValidator : LocalizedValidator<RemoveUserFromClientCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RemoveUserFromClientValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ClientIdInvalid"]);
        RuleFor(x => x.MemberUserId).NotEmpty().WithMessage(L["ClientUserIdRequired"]);
    }
}
