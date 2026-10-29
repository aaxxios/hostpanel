using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table configuration for <see cref="MigrationJob"/>.</summary>
public sealed class MigrationJobConfiguration : IEntityTypeConfiguration<MigrationJob>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<MigrationJob> builder)
    {
        builder.HasKey(x => x.Id);

        // Optimistic concurrency on PostgreSQL's system `xmin` column, mapped the same way as on
        // Invoice: two admins starting the same job at once both read it pending, and only the first
        // save may queue it, so only one pull is ever dispatched for a job.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.Property(x => x.Key).IsRequired().HasMaxLength(64);
        builder.HasIndex(x => x.Key).IsUnique();

        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Label).HasMaxLength(200);
        builder.Property(x => x.Failure).HasColumnName("ErrorMessage").HasConversion(new MigrationJobFailureConverter()).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.SourceUrl).IsRequired().HasMaxLength(500).HasDefaultValue(string.Empty);

        builder.Property(x => x.ExportClients).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportInvoices).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportServices).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportDomains).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportTickets).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportProducts).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportOrders).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportTransactions).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportQuotes).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportKnowledgebase).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportContacts).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportTicketReplies).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportAnnouncements).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportDownloads).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.ExportNetworkIssues).IsRequired().HasDefaultValue(true);

        builder.Property(x => x.AnnouncementsTotal).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.AnnouncementsImported).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.AnnouncementsSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.DownloadsTotal).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.DownloadsImported).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.DownloadsSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.NetworkIssuesTotal).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.NetworkIssuesImported).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.NetworkIssuesSkipped).IsRequired().HasDefaultValue(0);

        builder.Property(x => x.ClientsSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.InvoicesSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ServicesSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.DomainsSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.TicketsSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ProductsSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.OrdersSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.TransactionsSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.QuotesSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.KnowledgebaseSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ContactsSkipped).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.TicketRepliesSkipped).IsRequired().HasDefaultValue(0);

        builder.Property(x => x.ClientsFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.InvoicesFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ServicesFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.DomainsFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.TicketsFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ProductsFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.OrdersFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.TransactionsFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.QuotesFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.KnowledgebaseFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ContactsFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.TicketRepliesFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.AnnouncementsFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.DownloadsFailed).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.NetworkIssuesFailed).IsRequired().HasDefaultValue(0);
    }
}
