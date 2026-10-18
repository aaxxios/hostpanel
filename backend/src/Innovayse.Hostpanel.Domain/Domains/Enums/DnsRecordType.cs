namespace Innovayse.Hostpanel.Domain.Domains.Enums;
/// <summary>Supported DNS record types.</summary>
public enum DnsRecordType
{
    /// <summary>IPv4 address record.</summary>
    A = 0,

    /// <summary>IPv6 address record.</summary>
    AAAA = 1,

    /// <summary>Canonical name (alias) record.</summary>
    CNAME = 2,

    /// <summary>Mail exchange record.</summary>
    MX = 3,

    /// <summary>Text record — used for SPF, DKIM, verification, etc.</summary>
    TXT = 4,

    /// <summary>Name server record.</summary>
    NS = 5,

    /// <summary>Service locator record.</summary>
    SRV = 6,
}
