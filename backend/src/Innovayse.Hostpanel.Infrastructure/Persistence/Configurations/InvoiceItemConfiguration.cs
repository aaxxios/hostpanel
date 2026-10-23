using Innovayse.Hostpanel.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;
/// <summary>EF Core configuration for the <see cref="InvoiceItem"/> entity.</summary>
public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    /// <summary>Configures the <c>InvoiceItems</c> table mapping.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceId).IsRequired();

        // Nullable, unindexed-by-default no longer: "the invoices for this service" filters on
        // it, and without an index that read is a scan of every line ever billed. No FK
        // constraint is declared -- a terminated service's row may be removed while the invoice
        // that charged for it must survive as a financial record, and a constraint would either
        // block that or cascade the invoice line away with it.
        builder.Property(x => x.ClientServiceId);
        builder.HasIndex(x => x.ClientServiceId);

        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.Amount).HasColumnType("numeric(18,4)").IsRequired();
    }
}
