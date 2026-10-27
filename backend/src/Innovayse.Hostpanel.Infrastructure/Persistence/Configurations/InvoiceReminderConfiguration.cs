using Innovayse.Hostpanel.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF mapping for <see cref="InvoiceReminder"/>.</summary>
public sealed class InvoiceReminderConfiguration : IEntityTypeConfiguration<InvoiceReminder>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<InvoiceReminder> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InvoiceId).IsRequired();
        builder.Property(x => x.DaysBefore).IsRequired();
        builder.Property(x => x.SentAt).IsRequired();

        // One reminder per invoice per threshold; the unique index is what makes a re-run harmless.
        builder.HasIndex(x => new { x.InvoiceId, x.DaysBefore }).IsUnique();
        builder.HasOne<Invoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}
