using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SyncPendingDomainStatuses;

/// <summary>Validates <see cref="SyncPendingDomainStatusesCommand"/>: no rules; the command carries no input.</summary>
public sealed class SyncPendingDomainStatusesValidator : LocalizedValidator<SyncPendingDomainStatusesCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SyncPendingDomainStatusesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
