using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSalesTaxReport;

/// <summary>
/// Validator for <see cref="GetSalesTaxReportQuery"/>. Both bounds are plain <c>WHERE</c> filters
/// with no day-by-day iteration behind them, so an inverted range answers with an empty report
/// rather than crashing; there is nothing here worth refusing before the handler runs.
/// </summary>
public sealed class GetSalesTaxReportValidator : LocalizedValidator<GetSalesTaxReportQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetSalesTaxReportValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
