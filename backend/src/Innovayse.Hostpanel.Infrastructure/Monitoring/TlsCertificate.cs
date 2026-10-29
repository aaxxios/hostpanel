namespace Innovayse.Hostpanel.Infrastructure.Monitoring;

/// <summary>The certificate a host presented on its TLS port.</summary>
/// <param name="Issuer">The issuer's organisation, else its common name, else the raw issuer text.</param>
/// <param name="NotBefore">When the certificate's validity starts.</param>
/// <param name="NotAfter">When the certificate's validity ends.</param>
/// <param name="IsTrusted">Whether the platform accepted the chain and the host name; the certificate is read either way.</param>
internal sealed record TlsCertificate(string Issuer, DateTimeOffset NotBefore, DateTimeOffset NotAfter, bool IsTrusted);
