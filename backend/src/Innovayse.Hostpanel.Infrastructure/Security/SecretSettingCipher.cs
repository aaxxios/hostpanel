using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Security;

/// <summary>
/// <see cref="ISecretSettingCipher"/> over the same AES key that protects server credentials
/// (<see cref="IEncryptionService"/>). A protected value is <see cref="Marker"/> followed by the
/// Base64 ciphertext, so an encrypted row is recognised without trying to decrypt it.
/// </summary>
/// <remarks>
/// The API refuses to start without an encryption key in every environment, so the service is
/// always present. A value without the marker is a row written before protection existed; it is
/// returned as stored and protected the next time it is saved.
/// </remarks>
public sealed class SecretSettingCipher : ISecretSettingCipher
{
    /// <summary>Prefix that marks a stored value as protected by this cipher, version 1.</summary>
    public const string Marker = "enc:v1:";

    /// <summary>The AES service that encrypts and decrypts the values.</summary>
    private readonly IEncryptionService _encryption;

    /// <summary>Initializes the cipher.</summary>
    /// <param name="encryption">The AES service.</param>
    public SecretSettingCipher(IEncryptionService encryption)
    {
        ArgumentNullException.ThrowIfNull(encryption);
        _encryption = encryption;
    }

    /// <inheritdoc/>
    public string Protect(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        if (plaintext.Length == 0 || IsProtected(plaintext))
        {
            return plaintext;
        }

        return Marker + _encryption.Encrypt(plaintext);
    }

    /// <inheritdoc/>
    public string Reveal(string stored)
    {
        ArgumentNullException.ThrowIfNull(stored);

        if (!IsProtected(stored))
        {
            return stored;
        }

        return _encryption.Decrypt(stored[Marker.Length..]);
    }

    /// <inheritdoc/>
    public bool IsProtected(string stored)
    {
        ArgumentNullException.ThrowIfNull(stored);

        return stored.StartsWith(Marker, StringComparison.Ordinal);
    }
}
