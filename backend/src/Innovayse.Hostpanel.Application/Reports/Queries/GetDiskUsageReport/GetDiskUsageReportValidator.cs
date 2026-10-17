using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDiskUsageReport;

/// <summary>
/// Validator for <see cref="GetDiskUsageReportQuery"/>. The query carries no input, so it declares
/// no rules; the absence was decided, not forgotten.
/// </summary>
public sealed class GetDiskUsageReportValidator : LocalizedValidator<GetDiskUsageReportQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetDiskUsageReportValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
