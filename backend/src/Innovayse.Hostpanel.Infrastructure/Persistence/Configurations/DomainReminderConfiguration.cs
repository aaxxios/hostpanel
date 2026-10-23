using Innovayse.Hostpanel.Domain.Domains;
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
        builder.Property(r => r.ReminderType).IsRequired().HasMaxLength(100);
        builder.Property(r => r.SentTo).IsRequired().HasMaxLength(320);
        builder.HasIndex(r => r.DomainId);
    }
}
