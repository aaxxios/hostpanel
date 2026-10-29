using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="OrderItem"/>.</summary>
public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.BillingCycle).IsRequired().HasConversion(new BillingCycleConverter()).HasMaxLength(20);
        builder.Property(x => x.Domain).HasMaxLength(253);
        builder.Property(x => x.Hostname).HasMaxLength(253);
        builder.Property(x => x.FirstPaymentAmount).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.RecurringAmount).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.DomainAction).HasConversion(new DomainActionConverter()).HasMaxLength(20);
        builder.Property(x => x.EppCode).HasMaxLength(255);
        builder.Property(x => x.Years);

        // A product that an order line was placed for cannot be deleted; the line keeps the product name too.
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
