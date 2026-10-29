namespace Innovayse.Hostpanel.Infrastructure.Security;

/// <summary>
/// The encryption service for a context that only builds the model: the design-time factory the
/// migration tooling uses, and tests that read the model or run migrations. It holds no key and
/// refuses to encrypt or decrypt, so it can never write a protected column in plain text or in a
/// form no real key can read.
/// </summary>
public sealed class ModelOnlyEncryptionService : IEncryptionService
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Always: this service holds no key.</exception>
    public string Encrypt(string plaintext)
    {
        throw new InvalidOperationException(
            "This context was built without an encryption key, so it cannot write protected values.");
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Always: this service holds no key.</exception>
    public string Decrypt(string ciphertext)
    {
        throw new InvalidOperationException(
            "This context was built without an encryption key, so it cannot read protected values.");
    }
}
