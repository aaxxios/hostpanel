using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="Transaction"/> entity.</summary>
public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    /// <summary>Configures the <c>Transactions</c> table mapping.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId).IsRequired();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.TransactionId).HasMaxLength(255).IsRequired();
        builder.Property(x => x.InvoiceId);
        builder.Property(x => x.PaymentMethod).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AmountIn).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.AmountOut).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.Fees).HasColumnType("numeric(18,4)").IsRequired().HasDefaultValue(0m);
        builder.Property(x => x.AddedToCredit).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(CurrencyCode.Length);

        // The ledger lists newest first, per client or across all of them, and every report ranges
        // over the date; an invoice's payments and a gateway reference are looked up on their own.
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => new { x.ClientId, x.Date });
        builder.HasIndex(x => x.TransactionId);

        // A client who still has this record cannot be deleted: billing, service and support history
        // is kept, so the database refuses the delete instead of leaving rows without an owner.
        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // A ledger row outlives the invoice it settled: deleting a Draft or Cancelled invoice only clears the link.
        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
