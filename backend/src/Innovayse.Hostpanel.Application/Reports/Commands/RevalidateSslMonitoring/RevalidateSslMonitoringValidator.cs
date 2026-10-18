using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Commands.RevalidateSslMonitoring;

/// <summary>
/// Validates <see cref="RevalidateSslMonitoringCommand"/>. Its only field is a boolean that is
/// valid either way, so there is deliberately nothing to check; the class exists so the use case
/// is a complete set and the next reader knows this was decided rather than forgotten.
/// </summary>
public sealed class RevalidateSslMonitoringValidator : LocalizedValidator<RevalidateSslMonitoringCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RevalidateSslMonitoringValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
