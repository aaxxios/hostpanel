using Innovayse.Hostpanel.Domain.Ssl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;
/// <summary>EF Core table configuration for <see cref="SslCheck"/>.</summary>
public sealed class SslCheckConfiguration : IEntityTypeConfiguration<SslCheck>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SslCheck> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DomainName).IsRequired().HasMaxLength(253);
        builder.HasIndex(x => x.DomainName).IsUnique();
        builder.Property(x => x.HasSsl).IsRequired();
        builder.Property(x => x.Issuer).HasMaxLength(500);
        builder.Property(x => x.ExpiresAt);
        builder.Property(x => x.CheckedAt).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
    }
}
