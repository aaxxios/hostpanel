using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.SyncServerAccountCountsCron;

/// <summary>Validates <see cref="SyncServerAccountCountsCronCommand"/>: no rules; the command carries no input.</summary>
public sealed class SyncServerAccountCountsCronValidator : LocalizedValidator<SyncServerAccountCountsCronCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SyncServerAccountCountsCronValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
