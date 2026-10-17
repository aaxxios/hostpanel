using Innovayse.Hostpanel.Domain.Billing;

using Innovayse.Hostpanel.Domain.Billing.Enums;
namespace Innovayse.Hostpanel.Application.Reports.Extensions;

/// <summary>
/// Shared filtering for the Invoices report, used by both the paged view
/// (<c>Queries/InvoicesReport</c>) and its CSV export (<c>Queries/ExportInvoicesReport</c>) so the
/// two always answer with exactly the same rows.
/// </summary>
public static class InvoiceReportFilterExtensions
{
    /// <summary>Applies the Invoices report's optional filters to an invoice query.</summary>
    /// <param name="invoices">The base invoice query.</param>
    /// <param name="status">
    /// Invoice status to keep, matched case-insensitively; ignored (every status kept) when
    /// <see langword="null"/>, blank or not one of <see cref="InvoiceStatus"/>'s names.
    /// </param>
    /// <param name="createdFrom">Earliest creation date to include, inclusive.</param>
    /// <param name="createdTo">Latest creation date to include, inclusive.</param>
    /// <param name="dueFrom">Earliest due date to include, inclusive.</param>
    /// <param name="dueTo">Latest due date to include, inclusive.</param>
    /// <param name="paidFrom">Earliest paid date to include, inclusive. Invoices never paid never match.</param>
    /// <param name="paidTo">Latest paid date to include, inclusive. Invoices never paid never match.</param>
    /// <returns>The filtered query, not yet materialized.</returns>
    public static IQueryable<Invoice> ApplyReportFilters(
        this IQueryable<Invoice> invoices,
        string? status,
        DateOnly? createdFrom,
        DateOnly? createdTo,
        DateOnly? dueFrom,
        DateOnly? dueTo,
        DateOnly? paidFrom,
        DateOnly? paidTo)
    {
        ArgumentNullException.ThrowIfNull(invoices);

        var query = invoices;

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(i => i.Status == parsedStatus);
        }

        if (createdFrom.HasValue)
        {
            query = query.Where(i => i.CreatedAt >= createdFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (createdTo.HasValue)
        {
            query = query.Where(i => i.CreatedAt <= createdTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        }

        if (dueFrom.HasValue)
        {
            query = query.Where(i => i.DueDate >= dueFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (dueTo.HasValue)
        {
            query = query.Where(i => i.DueDate <= dueTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        }

        if (paidFrom.HasValue)
        {
            query = query.Where(i => i.PaidAt != null && i.PaidAt >= paidFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (paidTo.HasValue)
        {
            query = query.Where(i => i.PaidAt != null && i.PaidAt <= paidTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        }

        return query;
    }
}
