using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Domains.Events;

/// <summary>
/// A paid domain renewal could not be applied at the registrar (or was found already renewed), or a
/// renewal invoice could not be priced. Staff are told; the client already paid or was not billed.
/// </summary>
/// <param name="DomainId">The domain.</param>
/// <param name="ClientId">Its owner.</param>
/// <param name="DomainName">The domain name, for the mail.</param>
/// <param name="InvoiceId">The renewal invoice, or 0 when none was raised.</param>
/// <param name="Reason">What went wrong, for staff only.</param>
public record DomainRenewalFailedEvent(int DomainId, int ClientId, string DomainName, int InvoiceId, string Reason) : IDomainEvent
{
    /// <summary>Gets when it happened.</summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
