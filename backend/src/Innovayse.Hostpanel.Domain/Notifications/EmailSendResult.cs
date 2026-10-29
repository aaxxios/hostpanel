using Innovayse.Hostpanel.Domain.Notifications.Enums;

namespace Innovayse.Hostpanel.Domain.Notifications;

/// <summary>What the mail relay answered to one message.</summary>
/// <param name="Failure">Why the relay did not accept it; <see langword="null"/> when it did.</param>
public sealed record EmailSendResult(EmailSendFailure? Failure)
{
    /// <summary>The answer for a message the relay accepted.</summary>
    public static EmailSendResult Sent { get; } = new((EmailSendFailure?)null);

    /// <summary>Gets a value indicating whether the relay accepted the message.</summary>
    public bool Delivered => Failure is null;

    /// <summary>The answer for a message the relay did not accept.</summary>
    /// <param name="failure">Why it did not.</param>
    /// <returns>A result carrying <paramref name="failure"/>.</returns>
    public static EmailSendResult Failed(EmailSendFailure failure) => new(failure);
}
