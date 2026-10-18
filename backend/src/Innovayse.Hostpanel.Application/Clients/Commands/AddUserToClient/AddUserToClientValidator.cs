using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Microsoft.Extensions.Localization;
namespace Innovayse.Hostpanel.Application.Clients.Commands.AddUserToClient;

/// <summary>Validates <see cref="AddUserToClientCommand"/> before the handler executes.</summary>
public sealed class AddUserToClientValidator : LocalizedValidator<AddUserToClientCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AddUserToClientValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ClientIdInvalid"]);
        RuleFor(x => x.MemberUserId).NotEmpty().WithMessage(L["ClientUserIdRequired"]);
        // Checked here, in the shape a bit-flags integer is: any bit outside the known set is a
        // malformed request, not a state the handler needs a repository to evaluate.
        RuleFor(x => x.Permissions)
            .Must(p => (p & ~(int)ClientPermission.All) == 0)
            .WithMessage(L["ClientPermissionsInvalid"]);
    }
}
