using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="DomainReminder"/> entity.
/// Maps to the <c>DomainReminders</c> table.
/// </summary>
internal sealed class DomainReminderConfiguration : IEntityTypeConfiguration<DomainReminder>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DomainReminder> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.DomainId).IsRequired();
        builder.Property(r => r.ReminderType)
            .HasConversion(new DomainReminderTypeConverter())
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(r => r.SentTo).IsRequired().HasMaxLength(320);

        // A domain's reminders are loaded by domain, and "was this reminder sent" asks by type and date;
        // the composite's DomainId prefix also serves the plain per-domain load.
        builder.HasIndex(r => new { r.DomainId, r.ReminderType, r.SentAt });
    }
}
