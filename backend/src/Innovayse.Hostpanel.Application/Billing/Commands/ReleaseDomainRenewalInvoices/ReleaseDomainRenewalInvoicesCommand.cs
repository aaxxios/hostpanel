namespace Innovayse.Hostpanel.Application.Billing.Commands.ReleaseDomainRenewalInvoices;

/// <summary>
/// Takes a domain off every open auto-renewal invoice that bills it, before the domain is renewed
/// by a separate order, so the same year is never billed twice.
/// </summary>
/// <param name="RegisteredDomainId">The domain about to be renewed by order.</param>
public sealed record ReleaseDomainRenewalInvoicesCommand(int RegisteredDomainId);
