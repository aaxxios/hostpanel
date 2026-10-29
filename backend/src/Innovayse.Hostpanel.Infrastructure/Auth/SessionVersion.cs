using System.Security.Cryptography;
using System.Text;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// Derives the session version an access token carries from the account's security stamp.
/// </summary>
/// <remarks>
/// The stamp itself never goes into a token: an access token is signed, not encrypted, and the
/// stamp also keys the account's one-time tokens. A one-way digest changes whenever the stamp
/// changes and says nothing about it.
/// </remarks>
public static class SessionVersion
{
    /// <summary>Bytes of the digest kept in the token; enough to make a collision irrelevant.</summary>
    private const int VersionBytes = 16;

    /// <summary>Derives the version from a security stamp.</summary>
    /// <param name="securityStamp">The account's current security stamp.</param>
    /// <returns>A URL-safe Base64 digest of the stamp.</returns>
    public static string FromSecurityStamp(string securityStamp)
    {
        ArgumentNullException.ThrowIfNull(securityStamp);

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(securityStamp));
        return Convert.ToBase64String(digest, 0, VersionBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
