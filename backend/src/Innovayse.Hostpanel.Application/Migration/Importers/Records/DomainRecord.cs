namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One domain as the source plugin describes it.</summary>
/// <param name="ClientEmail">The client email.</param>
/// <param name="DomainName">The domain name.</param>
/// <param name="RegisteredAt">The registered at.</param>
/// <param name="ExpiresAt">The expires at.</param>
/// <param name="NextDueDate">The next due date.</param>
/// <param name="Registrar">The registrar.</param>
/// <param name="Nameservers">The nameservers entries.</param>
/// <param name="Status">The status.</param>
/// <param name="AutoRenew">Whether auto renew.</param>
/// <param name="RecurringAmount">The recurring amount.</param>
/// <param name="FirstPaymentAmount">The first payment amount.</param>
/// <param name="PaymentMethod">The payment method.</param>
/// <param name="SubscriptionId">The subscription identifier.</param>
/// <param name="AdminNotes">The admin notes.</param>
internal sealed record DomainRecord(
    string ClientEmail, string DomainName,
    DateTimeOffset RegisteredAt, DateTimeOffset ExpiresAt,
    DateTimeOffset? NextDueDate, string? Registrar, List<string>? Nameservers,
    string? Status, bool AutoRenew, decimal RecurringAmount, decimal FirstPaymentAmount,
    string? PaymentMethod, string? SubscriptionId, string? AdminNotes);
