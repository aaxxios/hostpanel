using Innovayse.Hostpanel.Application.Migration.Common;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.Migration;

/// <summary>
/// The totals answer exactly as the source plugin sends it: one flat count per record kind. It is
/// mapped to the grouped <see cref="MigrationSourceTotals"/> so the plugin's output stays as it is.
/// </summary>
internal sealed record MigrationSourceTotalsWire
{
    /// <summary>Client accounts available to pull.</summary>
    public int Clients { get; init; }

    /// <summary>Invoices available to pull.</summary>
    public int Invoices { get; init; }

    /// <summary>Provisioned client services available to pull.</summary>
    public int Services { get; init; }

    /// <summary>Registered domains available to pull.</summary>
    public int Domains { get; init; }

    /// <summary>Support tickets available to pull.</summary>
    public int Tickets { get; init; }

    /// <summary>Products available to pull.</summary>
    public int Products { get; init; }

    /// <summary>Orders available to pull.</summary>
    public int Orders { get; init; }

    /// <summary>Payment transactions available to pull.</summary>
    public int Transactions { get; init; }

    /// <summary>Quotes available to pull.</summary>
    public int Quotes { get; init; }

    /// <summary>Knowledgebase articles available to pull.</summary>
    public int Knowledgebase { get; init; }

    /// <summary>Client sub-contacts available to pull.</summary>
    public int Contacts { get; init; }

    /// <summary>Replies on support tickets available to pull.</summary>
    public int TicketReplies { get; init; }

    /// <summary>Announcements available to pull.</summary>
    public int Announcements { get; init; }

    /// <summary>Downloads available to pull.</summary>
    public int Downloads { get; init; }

    /// <summary>Download categories available to pull.</summary>
    public int DownloadCategories { get; init; }

    /// <summary>Network issues available to pull.</summary>
    public int NetworkIssues { get; init; }

    /// <summary>Groups the flat counts by what the records are.</summary>
    /// <returns>The grouped totals.</returns>
    public MigrationSourceTotals ToTotals() => new(
        new MigrationAccountTotals(Clients, Contacts),
        new MigrationBillingTotals(Invoices, Orders, Transactions, Quotes),
        new MigrationHostingTotals(Products, Services, Domains),
        new MigrationSupportTotals(Tickets, TicketReplies, NetworkIssues),
        new MigrationContentTotals(Knowledgebase, Announcements, Downloads, DownloadCategories));
}
