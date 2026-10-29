namespace Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap;

/// <summary>One email forwarding rule as Namecheap stores it.</summary>
/// <param name="Mailbox">The local mailbox part that is forwarded.</param>
/// <param name="ForwardTo">The address mail is forwarded to.</param>
internal sealed record NamecheapEmailForward(string Mailbox, string ForwardTo);
