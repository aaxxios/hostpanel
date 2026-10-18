using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.AutoRenewDomains;

/// <summary>Validates <see cref="AutoRenewDomainsCommand"/>: no rules — the command carries no input.</summary>
public sealed class AutoRenewDomainsValidator : LocalizedValidator<AutoRenewDomainsCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AutoRenewDomainsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
