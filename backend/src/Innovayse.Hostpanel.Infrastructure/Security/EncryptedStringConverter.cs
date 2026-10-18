using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Innovayse.Hostpanel.Infrastructure.Security;
/// <summary>
/// EF Core value converter that transparently encrypts strings when writing to the database
/// and decrypts them when reading. Null values pass through unchanged.
/// </summary>
public sealed class EncryptedStringConverter : ValueConverter<string?, string?>
{
    /// <summary>
    /// Initialises the converter with the specified encryption service.
    /// </summary>
    /// <param name="encryption">The encryption service to use for encrypt/decrypt operations.</param>
    public EncryptedStringConverter(IEncryptionService encryption)
        : base(
            v => v != null ? encryption.Encrypt(v) : null,
            v => v != null ? encryption.Decrypt(v) : null)
    {
    }
}
