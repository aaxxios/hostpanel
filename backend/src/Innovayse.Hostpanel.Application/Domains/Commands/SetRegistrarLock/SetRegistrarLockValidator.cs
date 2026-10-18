using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SetRegistrarLock;

/// <summary>Validates <see cref="SetRegistrarLockCommand"/> inputs before the handler executes.</summary>
public sealed class SetRegistrarLockValidator : LocalizedValidator<SetRegistrarLockCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SetRegistrarLockValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);
    }
}
