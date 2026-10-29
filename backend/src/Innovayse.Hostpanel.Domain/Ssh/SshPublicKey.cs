using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Innovayse.Hostpanel.Domain.Ssh;

/// <summary>
/// A validated OpenSSH ED25519 public key, reduced to its canonical form.
/// </summary>
/// <remarks>
/// <para>
/// Only ED25519 is accepted. The base64 blob is decoded and its structure checked byte by byte
/// (the algorithm name, then exactly 32 bytes of key), and the canonical line is re-encoded from
/// those bytes. The customer's comment is discarded: nothing they typed beyond the key itself
/// ever reaches a server's <c>authorized_keys</c>.
/// </para>
/// <para>
/// Anything that looks like a private key is refused outright, with its own reason, so the portal
/// can tell a customer who pasted the wrong file what went wrong without ever echoing it.
/// </para>
/// </remarks>
public sealed class SshPublicKey
{
    /// <summary>The only algorithm accepted.</summary>
    public const string Algorithm = "ssh-ed25519";

    /// <summary>Upper bound on input length; a real line is under 120 characters.</summary>
    private const int MaxInputLength = 1024;

    /// <summary>Gets the canonical line, <c>ssh-ed25519 &lt;base64&gt;</c>, with no comment.</summary>
    public string Canonical { get; }

    /// <summary>Gets the OpenSSH fingerprint, <c>SHA256:&lt;unpadded base64&gt;</c>.</summary>
    public string Fingerprint { get; }

    /// <summary>Initialises a parsed key.</summary>
    /// <param name="canonical">Canonical line.</param>
    /// <param name="fingerprint">SHA256 fingerprint.</param>
    private SshPublicKey(string canonical, string fingerprint)
    {
        Canonical = canonical;
        Fingerprint = fingerprint;
    }

    /// <summary>Parses a public key line as a customer pastes it.</summary>
    /// <param name="input">The pasted text.</param>
    /// <param name="key">The parsed key when valid.</param>
    /// <param name="looksLikePrivateKey">Whether the input was refused because it is a private key.</param>
    /// <returns><see langword="true"/> when the input is a valid ED25519 public key.</returns>
    public static bool TryParse(string? input, out SshPublicKey? key, out bool looksLikePrivateKey)
    {
        key = null;
        looksLikePrivateKey = false;

        if (string.IsNullOrWhiteSpace(input) || input.Length > MaxInputLength)
        {
            return false;
        }

        if (input.Contains("PRIVATE KEY", StringComparison.Ordinal))
        {
            looksLikePrivateKey = true;
            return false;
        }

        var trimmed = input.Trim();

        // One line only: a second line would be a second authorized_keys entry.
        if (trimmed.Contains('\n') || trimmed.Contains('\r'))
        {
            return false;
        }

        var parts = trimmed.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || parts[0] != Algorithm)
        {
            return false;
        }

        byte[] blob;
        try
        {
            blob = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (!IsEd25519Blob(blob))
        {
            return false;
        }

        var fingerprint = "SHA256:" + Convert.ToBase64String(SHA256.HashData(blob)).TrimEnd('=');
        key = new SshPublicKey($"{Algorithm} {Convert.ToBase64String(blob)}", fingerprint);
        return true;
    }

    /// <summary>Checks the wire format: string "ssh-ed25519", then a 32-byte string, and nothing else.</summary>
    /// <param name="blob">Decoded key blob.</param>
    /// <returns><see langword="true"/> when well-formed.</returns>
    private static bool IsEd25519Blob(byte[] blob)
    {
        var name = Encoding.ASCII.GetBytes(Algorithm);
        if (blob.Length != 4 + name.Length + 4 + 32)
        {
            return false;
        }

        var span = blob.AsSpan();
        return BinaryPrimitives.ReadUInt32BigEndian(span) == name.Length
            && span.Slice(4, name.Length).SequenceEqual(name)
            && BinaryPrimitives.ReadUInt32BigEndian(span[(4 + name.Length)..]) == 32;
    }
}
