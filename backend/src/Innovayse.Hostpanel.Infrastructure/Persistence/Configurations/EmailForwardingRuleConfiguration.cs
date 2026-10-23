using Innovayse.Hostpanel.Domain.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;
/// <summary>
/// EF Core configuration for the <see cref="EmailForwardingRule"/> entity.
/// Maps to the <c>EmailForwardingRules</c> table.
/// </summary>
internal sealed class EmailForwardingRuleConfiguration : IEntityTypeConfiguration<EmailForwardingRule>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EmailForwardingRule> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.DomainId).IsRequired();
        builder.Property(r => r.Source).IsRequired().HasMaxLength(255);
        builder.Property(r => r.Destination).IsRequired().HasMaxLength(320);
        builder.Property(r => r.IsActive).HasDefaultValue(true);
        builder.HasIndex(r => r.DomainId);
    }
}
