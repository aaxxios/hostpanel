using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.CheckDomainExpiries;

/// <summary>Validates <see cref="CheckDomainExpiriesCommand"/>: no rules — the command carries no input.</summary>
public sealed class CheckDomainExpiriesValidator : LocalizedValidator<CheckDomainExpiriesCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CheckDomainExpiriesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
