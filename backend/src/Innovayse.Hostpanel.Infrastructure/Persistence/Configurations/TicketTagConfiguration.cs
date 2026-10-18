using Innovayse.Hostpanel.Domain.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;
/// <summary>EF Core configuration for the <see cref="TicketTag"/> entity.</summary>
public sealed class TicketTagConfiguration : IEntityTypeConfiguration<TicketTag>
{
    /// <summary>Configures the <c>ticket_tags</c> table mapping.</summary>
    /// <param name="builder">Builder for the ticket tag entity.</param>
    public void Configure(EntityTypeBuilder<TicketTag> builder)
    {
        builder.ToTable("ticket_tags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TicketId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => new { x.TicketId, x.Name }).IsUnique();
        builder.HasIndex(x => x.Name);
    }
}
