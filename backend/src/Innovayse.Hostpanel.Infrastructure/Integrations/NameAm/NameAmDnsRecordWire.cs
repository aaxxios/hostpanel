using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;

/// <summary>One DNS record in the body that replaces a domain's zone.</summary>
/// <param name="Type">The record type (e.g. <c>A</c>, <c>MX</c>).</param>
/// <param name="Name">The host the record is for.</param>
/// <param name="Content">The record value.</param>
/// <param name="Ttl">Time to live in seconds.</param>
/// <param name="Action">What Name.am does with the record; every record of the full set is sent as <c>CREATE</c>.</param>
/// <param name="Priority">MX or SRV priority; sent as <c>null</c> when the type has none, as the API expects the key.</param>
internal sealed record NameAmDnsRecordWire(
    string Type,
    string Name,
    string Content,
    int Ttl,
    string Action,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)] int? Priority);
