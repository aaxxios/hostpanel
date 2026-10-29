using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetRevenueReport;

/// <summary>
///     Validates <see cref="GetRevenueReportQuery"/>. Any two dates are accepted, including a
///     start after the end: the handler answers an empty report rather than refusing it,
///     decided, not forgotten.
/// </summary>
public sealed class GetRevenueReportValidator : LocalizedValidator<GetRevenueReportQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetRevenueReportValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
