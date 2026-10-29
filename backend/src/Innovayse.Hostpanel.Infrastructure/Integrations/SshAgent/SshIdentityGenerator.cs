using System.Text;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Security;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

/// <summary>
/// <see cref="ISshIdentityGenerator"/> on BouncyCastle (already a dependency of SSH.NET): an ED25519
/// pair, the private half in the OpenSSH v1 format SSH.NET and OpenSSH both read.
/// </summary>
public sealed class SshIdentityGenerator : ISshIdentityGenerator
{
    /// <summary>Width of a base64 line in an OpenSSH private key file.</summary>
    private const int PemLineWidth = 70;

    /// <inheritdoc/>
    public SshIdentity Generate(string comment)
    {
        var generator = new Ed25519KeyPairGenerator();
        generator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
        var pair = generator.GenerateKeyPair();

        var privateBlob = OpenSshPrivateKeyUtilities.EncodePrivateKey(pair.Private);
        var pem = new StringBuilder("-----BEGIN OPENSSH PRIVATE KEY-----\n");
        var body = Convert.ToBase64String(privateBlob);
        for (var i = 0; i < body.Length; i += PemLineWidth)
        {
            pem.Append(body, i, Math.Min(PemLineWidth, body.Length - i)).Append('\n');
        }

        pem.Append("-----END OPENSSH PRIVATE KEY-----\n");

        var publicBlob = OpenSshPublicKeyUtilities.EncodePublicKey((Ed25519PublicKeyParameters)pair.Public);
        var safeComment = new string(comment.Where(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' or '@').ToArray());
        return new SshIdentity(pem.ToString(), $"ssh-ed25519 {Convert.ToBase64String(publicBlob)} {safeComment}".TrimEnd());
    }
}
