using Innovayse.Hostpanel.Domain.Domains;

namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// Adds or updates one DNS record in a domain's zone at the registrar.
/// </summary>
/// <param name="Domain">The domain whose zone is changed.</param>
/// <param name="Record">The DNS record to write (matched by ID or host and type when updating).</param>
public sealed record DnsRecordChange(DomainRef Domain, DnsRecord Record);
