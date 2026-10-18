using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.ExportInvoicesReport;

/// <summary>
/// Validator for <see cref="ExportInvoicesReportQuery"/>. Every field is the same kind of plain
/// filter the paged Invoices report accepts unvalidated, and there is no page size to bound here;
/// no rule is declared — decided, not forgotten.
/// </summary>
public sealed class ExportInvoicesReportValidator : LocalizedValidator<ExportInvoicesReportQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ExportInvoicesReportValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
