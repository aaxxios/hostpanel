using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.UpdateServerGroup;

/// <summary>Validates <see cref="UpdateServerGroupCommand"/> before it reaches the handler.</summary>
public sealed class UpdateServerGroupValidator : LocalizedValidator<UpdateServerGroupCommand>
{
    /// <summary>Longest a group name may be.</summary>
    private const int NameMaxLength = 200;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateServerGroupValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).MustBePositiveId(L["ServerGroupIdInvalid"]);

        RuleFor(x => x.Name).NotEmpty().WithMessage(L["ServerGroupNameRequired"])
            .MaximumLength(NameMaxLength).WithMessage(L["ServerGroupNameTooLong", NameMaxLength]);
    }
}
