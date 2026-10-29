namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// Deletes one DNS record from a domain's zone at the registrar.
/// </summary>
/// <param name="Domain">The domain whose zone is changed.</param>
/// <param name="RecordId">The ID of the DNS record to delete.</param>
public sealed record DnsRecordRemoval(DomainRef Domain, int RecordId);
