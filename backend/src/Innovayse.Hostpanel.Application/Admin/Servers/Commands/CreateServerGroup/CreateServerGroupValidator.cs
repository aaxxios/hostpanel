using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.CreateServerGroup;

/// <summary>Validates <see cref="CreateServerGroupCommand"/> before it reaches the handler.</summary>
public sealed class CreateServerGroupValidator : LocalizedValidator<CreateServerGroupCommand>
{
    /// <summary>Longest a group name may be.</summary>
    private const int NameMaxLength = 200;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateServerGroupValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(L["ServerGroupNameRequired"])
            .MaximumLength(NameMaxLength).WithMessage(L["ServerGroupNameTooLong", NameMaxLength]);
    }
}
