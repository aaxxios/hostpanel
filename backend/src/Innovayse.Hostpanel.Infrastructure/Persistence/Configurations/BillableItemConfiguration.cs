using Innovayse.Hostpanel.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;
/// <summary>EF Core configuration for the <see cref="BillableItem"/> entity.</summary>
public sealed class BillableItemConfiguration : IEntityTypeConfiguration<BillableItem>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<BillableItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId).IsRequired();
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Amount).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.RecurringPeriod).HasMaxLength(50);
        builder.Property(x => x.IsInvoiced).IsRequired();
        builder.Property(x => x.InvoiceId);
        builder.Property(x => x.NextDueDate);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
