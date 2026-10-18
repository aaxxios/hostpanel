using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSslMonitoring;

/// <summary>
/// Validator for <see cref="GetSslMonitoringQuery"/>. Its one input is a flag, which every value
/// of is valid, so no rule is declared — decided, not forgotten.
/// </summary>
public sealed class GetSslMonitoringValidator : LocalizedValidator<GetSslMonitoringQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetSslMonitoringValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
