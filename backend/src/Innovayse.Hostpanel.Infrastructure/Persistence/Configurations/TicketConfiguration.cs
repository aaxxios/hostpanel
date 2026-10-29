using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="Ticket"/> aggregate.</summary>
public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    /// <summary>Configures the <c>Tickets</c> table mapping.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId).IsRequired();
        builder.Property(x => x.Subject).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.DepartmentId);
        builder.Property(x => x.AssignedToStaffId);
        builder.Property(x => x.CreatedAt).IsRequired();

        // The client's tickets newest first, and the staff list filtered by status newest first.
        builder.HasIndex(x => new { x.ClientId, x.CreatedAt });
        builder.HasIndex(x => new { x.Status, x.CreatedAt });

        // Two staff replies or a reply racing a status change on the same ticket cannot both save
        // over each other; the second save fails with ConcurrencyConflictException.
        builder.UseXminConcurrencyToken();

        builder.Property(x => x.Rating);
        builder.Property(x => x.FeedbackComment).HasMaxLength(2000);
        builder.Property(x => x.FeedbackLeftBy).HasMaxLength(200);
        builder.Property(x => x.FeedbackAt);

        builder.HasMany(x => x.Replies)
            .WithOne()
            .HasForeignKey("TicketId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Replies)
            .HasField("_replies")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Tags)
            .WithOne()
            .HasForeignKey(t => t.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Tags)
            .HasField("_tags")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // A client who still has this record cannot be deleted: billing, service and support history
        // is kept, so the database refuses the delete instead of leaving rows without an owner.
        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // A ticket outlives its department: deleting the department leaves the ticket unrouted.
        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
