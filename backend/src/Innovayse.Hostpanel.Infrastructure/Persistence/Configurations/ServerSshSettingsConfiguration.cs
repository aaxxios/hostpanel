using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Infrastructure.Security;
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
        builder.Ignore(x => x.IsComplete);

        // The agent's private key is a server credential like Server.AccessHash and is encrypted
        // the same way. The width is sized for the encrypted form and is the same with or without
        // a key configured, so the model never depends on the environment it is built in.
        var privateKey = builder.Property(x => x.AgentPrivateKey).HasMaxLength(8000);
        var converter = EncryptionServiceHolder.CreateConverter();
        if (converter is not null)
        {
            privateKey.HasConversion(converter);
        }
    }
}
