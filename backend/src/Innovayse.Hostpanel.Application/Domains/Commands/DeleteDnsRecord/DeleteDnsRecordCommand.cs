namespace Innovayse.Hostpanel.Application.Domains.Commands.DeleteDnsRecord;

/// <summary>Command to remove a DNS record from a domain's zone.</summary>
/// <param name="DomainId">
/// Primary key of the domain that owns the record. Nullable so the route id can overlay it: a
/// non-nullable member here would make the body required before <c>with { DomainId = id }</c>
/// ever ran.
/// </param>
/// <param name="RecordId">Primary key of the DNS record to delete; overlaid from the route.</param>
public record DeleteDnsRecordCommand(int? DomainId, int? RecordId);
