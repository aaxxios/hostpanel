using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Innovayse.Hostpanel.Infrastructure.Monitoring;

/// <summary>Opens a TLS connection to vetted addresses of a host and reads the certificate it presents.</summary>
internal static class TlsCertificateReader
{
    /// <summary>The TLS port probed.</summary>
    private const int TlsPort = 443;

    /// <summary>How long connecting and the handshake may take together.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    /// <summary>Reads the certificate the host presents.</summary>
    /// <param name="host">The host name, sent as the server name and checked against the certificate.</param>
    /// <param name="addresses">The addresses to dial. They must already have passed <c>PublicHostGuard</c>: the name is not resolved again.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The certificate, or <see langword="null"/> when the host did not answer or presented none.</returns>
    public static async Task<TlsCertificate?> ReadAsync(string host, IReadOnlyList<IPAddress> addresses, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(Timeout);

        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync(addresses.ToArray(), TlsPort, timeout.Token);

            // The handshake is accepted whatever the policy says: the aim is to read the certificate,
            // and whether it is trusted is recorded beside it rather than refusing to look.
            var policyErrors = SslPolicyErrors.None;
            using var ssl = new SslStream(tcp.GetStream(), false, (_, _, _, errors) =>
            {
                policyErrors = errors;
                return true;
            });
            await ssl.AuthenticateAsClientAsync(new SslClientAuthenticationOptions { TargetHost = host }, timeout.Token);

            if (ssl.RemoteCertificate is not X509Certificate2 cert)
            {
                return null;
            }

            return new TlsCertificate(
                IssuerOf(cert),
                new DateTimeOffset(cert.NotBefore),
                new DateTimeOffset(cert.NotAfter),
                policyErrors == SslPolicyErrors.None);
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            // Unreachable, refused, timed out or not speaking TLS: all mean "no certificate to show".
            return null;
        }
    }

    /// <summary>Picks the issuer's organisation, falling back to its common name and then the raw text.</summary>
    /// <param name="cert">The certificate.</param>
    /// <returns>The issuer as a person would name it.</returns>
    private static string IssuerOf(X509Certificate2 cert)
    {
        var parts = cert.Issuer.Split(',').Select(p => p.Trim()).ToList();

        return parts.FirstOrDefault(p => p.StartsWith("O=", StringComparison.OrdinalIgnoreCase))?[2..]
            ?? parts.FirstOrDefault(p => p.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))?[3..]
            ?? cert.Issuer;
    }
}
