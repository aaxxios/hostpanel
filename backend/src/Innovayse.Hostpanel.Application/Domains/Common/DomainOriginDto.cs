namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>The order that created a domain and the service it is linked to.</summary>
/// <param name="OrderId">FK to the order that created this domain; null if unknown.</param>
/// <param name="OrderType">Order type: "Register" or "Transfer".</param>
/// <param name="LinkedServiceId">FK to linked service (e.g. hosting plan); null if none.</param>
public sealed record DomainOriginDto(int? OrderId, string OrderType, int? LinkedServiceId);
