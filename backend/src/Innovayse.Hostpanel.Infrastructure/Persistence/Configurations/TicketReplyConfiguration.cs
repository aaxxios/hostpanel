using Innovayse.Hostpanel.Domain.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="TicketReply"/> entity.</summary>
public sealed class TicketReplyConfiguration : IEntityTypeConfiguration<TicketReply>
{
    /// <summary>Configures the <c>TicketReplies</c> table mapping.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<TicketReply> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.AuthorName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.IsStaffReply).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property<int>("TicketId").IsRequired();
        builder.HasIndex("TicketId");
    }
}
