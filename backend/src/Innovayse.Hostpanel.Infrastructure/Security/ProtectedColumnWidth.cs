namespace Innovayse.Hostpanel.Infrastructure.Security;

/// <summary>
/// Sizes a column that holds an encrypted value. The stored form is the
/// <see cref="SecretSettingCipher.Marker"/>, then the Base64 of a 16-byte IV followed by the
/// AES-CBC ciphertext, which PKCS7 pads to the next whole 16-byte block (a full extra block when
/// the input is already aligned).
/// </summary>
public static class ProtectedColumnWidth
{
    /// <summary>The most bytes one character takes in UTF-8.</summary>
    private const int MaxBytesPerChar = 4;

    /// <summary>The AES block size and the IV length, in bytes.</summary>
    private const int BlockSize = 16;

    /// <summary>The raw bytes Base64 packs into one group of four characters.</summary>
    private const int Base64GroupBytes = 3;

    /// <summary>The Base64 characters one group holds.</summary>
    private const int Base64GroupChars = 4;

    /// <summary>
    /// Gets the column width that holds the encrypted form of the longest allowed plain value,
    /// whatever characters it is made of.
    /// </summary>
    /// <param name="maxPlainChars">The longest plain value the field accepts, in characters.</param>
    /// <returns>The <c>HasMaxLength</c> figure for the column.</returns>
    public static int For(int maxPlainChars)
    {
        var plainBytes = maxPlainChars * MaxBytesPerChar;
        var cipherBytes = ((plainBytes / BlockSize) + 1) * BlockSize;
        var rawBytes = BlockSize + cipherBytes;
        var base64Chars = (rawBytes + Base64GroupBytes - 1) / Base64GroupBytes * Base64GroupChars;

        return SecretSettingCipher.Marker.Length + base64Chars;
    }
}
