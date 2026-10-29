using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Innovayse.Hostpanel.Infrastructure.Security;

/// <summary>
/// EF Core value converter that encrypts strings when writing to the database and decrypts them
/// when reading. Null values pass through unchanged.
/// </summary>
/// <remarks>
/// Every value written carries <see cref="SecretSettingCipher.Marker"/>, so an encrypted row is
/// recognised without trying to decrypt it. A value without the marker is unmarked ciphertext or
/// a plain value stored before encryption was required, and <see cref="IEncryptionService.Decrypt"/>
/// hands back a plain value unchanged. Such a row stays readable, and is written back encrypted and marked the
/// next time its entity is saved with that property modified; nothing rewrites rows nobody saves.
/// </remarks>
public sealed class EncryptedStringConverter : ValueConverter<string?, string?>
{
    /// <summary>
    /// Initialises the converter with the specified encryption service.
    /// </summary>
    /// <param name="encryption">The encryption service to use for encrypt/decrypt operations.</param>
    public EncryptedStringConverter(IEncryptionService encryption)
        : base(
            v => v != null ? SecretSettingCipher.Marker + encryption.Encrypt(v) : null,
            v => v != null ? Reveal(encryption, v) : null)
    {
    }

    /// <summary>Reads a stored value, whether it is marked, earlier ciphertext or a legacy plain value.</summary>
    /// <param name="encryption">The encryption service.</param>
    /// <param name="stored">The value as stored.</param>
    /// <returns>The plain value.</returns>
    private static string Reveal(IEncryptionService encryption, string stored) =>
        stored.StartsWith(SecretSettingCipher.Marker, StringComparison.Ordinal)
            ? encryption.Decrypt(stored[SecretSettingCipher.Marker.Length..])
            : encryption.Decrypt(stored);
}
