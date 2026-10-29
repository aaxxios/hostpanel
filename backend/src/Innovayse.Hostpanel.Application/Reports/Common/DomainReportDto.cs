using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One row in the Domains report.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="OrderId">The order identifier.</param>
/// <param name="OrderType">The order type.</param>
/// <param name="DomainName">The domain name.</param>
/// <param name="Amounts">The first payment and recurring amounts.</param>
/// <param name="RegistrationPeriod">The registration period.</param>
/// <param name="Dates">The registration, expiry and next due dates.</param>
/// <param name="Registrar">The registrar.</param>
/// <param name="PaymentMethod">The payment method.</param>
/// <param name="Status">The status.</param>
/// <param name="Notes">The notes.</param>
public record DomainReportDto(
    int Id,
    int ClientId,
    string ClientName,
    int? OrderId,
    string OrderType,
    string DomainName,
    DomainReportAmountsDto Amounts,
    int RegistrationPeriod,
    DomainReportDatesDto Dates,
    string? Registrar,
    string? PaymentMethod,
    DomainStatus Status,
    string? Notes);
