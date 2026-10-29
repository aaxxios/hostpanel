using Innovayse.Hostpanel.Domain.Servers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for <see cref="ServerSshSettings"/> (table <c>ServerSshSettings</c>).</summary>
public sealed class ServerSshSettingsConfiguration : IEntityTypeConfiguration<ServerSshSettings>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ServerSshSettings> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ServerId).IsUnique();
        builder.HasOne<Server>().WithMany().HasForeignKey(x => x.ServerId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.SshHostname).IsRequired().HasMaxLength(255);
        builder.Property(x => x.AgentHost).IsRequired().HasMaxLength(255);
        builder.Property(x => x.AgentUsername).IsRequired().HasMaxLength(32);
        builder.Property(x => x.AgentHostKeyFingerprint).IsRequired().HasMaxLength(100);
        builder.Property(x => x.AgentPublicKey).HasMaxLength(200);
        builder.Ignore(x => x.IsComplete);

        // AgentPrivateKey is a server credential like Server.AccessHash: encrypted and sized by ProtectedColumns.
    }
}
