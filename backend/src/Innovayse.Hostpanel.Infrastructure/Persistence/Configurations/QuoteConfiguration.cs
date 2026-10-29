using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="Quote"/> aggregate.</summary>
public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(x => x.Id);

        // Two staff actions or a staff action and a client's accept or decline on the same quote
        // cannot both save over each other; the second save fails with ConcurrencyConflictException.
        builder.UseXminConcurrencyToken();

        // The client's quotes.
        builder.HasIndex(x => x.ClientId);

        builder.Property(x => x.ClientId).IsRequired();
        builder.Property(x => x.Subject).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Stage).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ExpiryDate).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.ProposalText).HasMaxLength(4000);
        builder.Property(x => x.CustomerNotes).HasMaxLength(4000);
        builder.Property(x => x.AdminNotes).HasMaxLength(4000);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(CurrencyCode.Length);
        builder.Property(x => x.Total).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Navigation: Quote owns a collection of QuoteItems via private backing field _items.
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // A client who still has this record cannot be deleted: billing, service and support history
        // is kept, so the database refuses the delete instead of leaving rows without an owner.
        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
