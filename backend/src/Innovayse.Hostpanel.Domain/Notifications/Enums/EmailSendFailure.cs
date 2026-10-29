namespace Innovayse.Hostpanel.Domain.Notifications.Enums;

/// <summary>Why the mail relay did not accept a message.</summary>
/// <remarks>
/// The stored form of each value is its name: never rename a member, rows already hold it. The
/// relay's own wording is logged by the adapter and never stored, because it can carry server
/// names, addresses and credential hints.
/// </remarks>
public enum EmailSendFailure
{
    /// <summary>A reason this list does not name, including rows written before the reason was recorded as a value.</summary>
    Unknown = 0,

    /// <summary>The relay could not be reached: no route, a refused connection or a dropped one.</summary>
    RelayUnreachable = 1,

    /// <summary>The relay did not answer in time.</summary>
    Timeout = 2,

    /// <summary>The encrypted connection to the relay could not be set up, for example an untrusted certificate.</summary>
    SecureConnectionFailed = 3,

    /// <summary>The relay refused the configured credentials.</summary>
    AuthenticationRejected = 4,

    /// <summary>The relay refused the recipient address, or the address could not be read.</summary>
    RecipientRejected = 5,

    /// <summary>The relay refused the sender address.</summary>
    SenderRejected = 6,

    /// <summary>The relay refused the message itself, for example for its size or content.</summary>
    MessageRejected = 7,
}
