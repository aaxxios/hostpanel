namespace Innovayse.Hostpanel.Domain.Domains.Enums;

/// <summary>Kind of expiry reminder recorded against a registered domain.</summary>
public enum DomainReminderType
{
    /// <summary>Warning sent thirty days before the domain expires.</summary>
    ThirtyDaysBeforeExpiry = 0,

    /// <summary>Warning sent fourteen days before the domain expires.</summary>
    FourteenDaysBeforeExpiry = 1,

    /// <summary>Warning sent seven days before the domain expires.</summary>
    SevenDaysBeforeExpiry = 2,

    /// <summary>Warning sent one day before the domain expires.</summary>
    OneDayBeforeExpiry = 3,

    /// <summary>Notice sent after the domain has expired.</summary>
    ExpiredNotice = 4,

    /// <summary>Notice sent while the domain is in the redemption period.</summary>
    RedemptionNotice = 5,

    /// <summary>
    /// A stored reminder type this version does not recognise. Only ever produced when reading a
    /// row back; nothing records a reminder of this kind.
    /// </summary>
    Unknown = 6,
}
