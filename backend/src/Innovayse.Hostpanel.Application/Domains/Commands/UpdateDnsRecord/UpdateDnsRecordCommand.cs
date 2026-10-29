namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateDnsRecord;

/// <summary>Command to update the value, TTL, and priority of an existing DNS record.</summary>
/// <param name="DomainId">
/// Primary key of the domain that owns the record. Nullable so the route id can overlay it: a
/// non-nullable member here would make the body required before <c>with { DomainId = id }</c>
/// ever ran.
/// </param>
/// <param name="RecordId">Primary key of the DNS record to update; overlaid from the route.</param>
/// <param name="Value">New record value.</param>
/// <param name="Ttl">New time-to-live in seconds.</param>
/// <param name="Priority">New priority for MX/SRV records; null to clear.</param>
public sealed record UpdateDnsRecordCommand(int? DomainId, int? RecordId, string Value, int Ttl, int? Priority);
