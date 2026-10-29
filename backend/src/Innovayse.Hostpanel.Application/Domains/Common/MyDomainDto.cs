using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// The client's own domain as the client area reads it. The registrar's internal reference, the
/// transfer code, the subscription id, the staff notes and the reminder log of
/// <see cref="DomainDto"/> are not part of it, so they cannot reach the client by any path; the
/// transfer code is only ever handed out through its own audited request.
/// </summary>
/// <param name="Id">Domain primary key.</param>
/// <param name="Name">Full domain name including the TLD.</param>
/// <param name="Status">Current lifecycle status.</param>
/// <param name="RegisteredAt">Registration date (UTC).</param>
/// <param name="ExpiresAt">Expiry date (UTC).</param>
/// <param name="NextDueDate">Next billing due date (UTC).</param>
/// <param name="AutoRenew">Whether the domain renews automatically.</param>
/// <param name="WhoisPrivacy">Whether WHOIS privacy is on.</param>
/// <param name="IsLocked">Whether the registrar lock is on.</param>
/// <param name="FirstPaymentAmount">Amount charged at registration.</param>
/// <param name="RecurringAmount">Amount charged at each renewal.</param>
/// <param name="PriceCurrency">ISO 4217 code of the two amounts.</param>
/// <param name="PaymentMethod">Payment method used for renewals; null when not set.</param>
/// <param name="RegistrationPeriod">Registration period in years.</param>
/// <param name="Management">The DNS, nameserver and forwarding settings the client manages.</param>
public sealed record MyDomainDto(
    int Id,
    string Name,
    DomainStatus Status,
    DateTimeOffset RegisteredAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset NextDueDate,
    bool AutoRenew,
    bool WhoisPrivacy,
    bool IsLocked,
    decimal FirstPaymentAmount,
    decimal RecurringAmount,
    string PriceCurrency,
    string? PaymentMethod,
    int RegistrationPeriod,
    MyDomainManagementDto Management);
