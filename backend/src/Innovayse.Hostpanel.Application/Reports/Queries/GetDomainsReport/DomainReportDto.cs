namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDomainsReport;

/// <summary>One row in the Domains report.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="OrderId">The order identifier.</param>
/// <param name="OrderType">The order type.</param>
/// <param name="DomainName">The domain name.</param>
/// <param name="FirstPaymentAmount">The first payment amount.</param>
/// <param name="RecurringAmount">The recurring amount.</param>
/// <param name="RegistrationPeriod">The registration period.</param>
/// <param name="RegistrationDate">The registration date.</param>
/// <param name="ExpiryDate">The expiry date.</param>
/// <param name="NextDueDate">The next due date.</param>
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
    decimal FirstPaymentAmount,
    decimal RecurringAmount,
    int RegistrationPeriod,
    string RegistrationDate,
    string ExpiryDate,
    string NextDueDate,
    string? Registrar,
    string? PaymentMethod,
    string Status,
    string? Notes);
