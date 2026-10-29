using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.ExportTransactionsReport;

/// <summary>
/// Validator for <see cref="ExportTransactionsReportQuery"/>. Every field is the same kind of
/// plain filter the paged Transactions report accepts unvalidated, and there is no page size to
/// bound here; no rule is declared: decided, not forgotten.
/// </summary>
public sealed class ExportTransactionsReportValidator : LocalizedValidator<ExportTransactionsReportQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ExportTransactionsReportValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
