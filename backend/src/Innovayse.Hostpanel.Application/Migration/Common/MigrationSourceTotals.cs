namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>
/// How many records of each kind the legacy source holds, as it reports them before a pull
/// starts, grouped by what the records are. The job uses these to size its progress counters, so
/// a count here is the denominator the admin UI shows, not a guarantee that every record will
/// import. <see cref="Empty"/> is the starting point: every group counts zero.
/// </summary>
/// <param name="Accounts">Client accounts and their sub-contacts.</param>
/// <param name="Billing">Invoices, orders, transactions and quotes.</param>
/// <param name="Hosting">Products, services and domains.</param>
/// <param name="Support">Tickets, ticket replies and network issues.</param>
/// <param name="Content">Knowledgebase articles, announcements and downloads.</param>
public sealed record MigrationSourceTotals(
    MigrationAccountTotals Accounts,
    MigrationBillingTotals Billing,
    MigrationHostingTotals Hosting,
    MigrationSupportTotals Support,
    MigrationContentTotals Content)
{
    /// <summary>Totals where every group counts zero.</summary>
    public static MigrationSourceTotals Empty { get; } = new(new(), new(), new(), new(), new());
}
