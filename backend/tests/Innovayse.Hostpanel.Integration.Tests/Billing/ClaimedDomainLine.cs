using Innovayse.Hostpanel.Domain.Domains;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// A domain whose renewal line is already claimed by an invoice.
/// </summary>
/// <param name="Domain">The domain.</param>
/// <param name="InvoiceId">The invoice holding the line.</param>
public sealed record ClaimedDomainLine(RegisteredDomain Domain, int InvoiceId);
