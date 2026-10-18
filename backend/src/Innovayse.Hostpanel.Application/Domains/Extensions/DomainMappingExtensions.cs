using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Domain.Domains;

namespace Innovayse.Hostpanel.Application.Domains.Extensions;

/// <summary>
/// Maps a <see cref="RegisteredDomain"/> to <see cref="DomainDto"/>. Every read of a full domain --
/// the admin detail and the client's own list -- goes through this one mapping, so a field added to
/// the DTO is added once rather than in places that quietly drift apart.
/// </summary>
public static class DomainMappingExtensions
{
    /// <summary>Maps a domain, with its already-loaded child collections, to its full DTO.</summary>
    /// <param name="domain">The domain; its nameservers, DNS records, forwarding rules and reminders must be loaded.</param>
    /// <returns>The mapped DTO.</returns>
    public static DomainDto ToDto(this RegisteredDomain domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new DomainDto(
            domain.Id,
            domain.ClientId,
            domain.Name,
            domain.Tld,
            domain.Status,
            domain.RegisteredAt,
            domain.ExpiresAt,
            domain.AutoRenew,
            domain.WhoisPrivacy,
            domain.IsLocked,
            domain.RegistrarRef,
            domain.EppCode,
            domain.LinkedServiceId,
            domain.FirstPaymentAmount,
            domain.RecurringAmount,
            domain.PaymentMethod,
            domain.PromotionCode,
            domain.SubscriptionId,
            domain.AdminNotes,
            domain.OrderId,
            domain.OrderType,
            domain.DnsManagement,
            domain.EmailForwarding,
            domain.PriceCurrency,
            domain.NextDueDate,
            domain.Registrar,
            domain.RegistrationPeriod,
            domain.Nameservers
                .Select(n => new NameserverDto(n.Id, n.Host))
                .ToList(),
            domain.DnsRecords
                .Select(r => new DnsRecordDto(r.Id, r.Type, r.Host, r.Value, r.Ttl, r.Priority))
                .ToList(),
            domain.EmailForwardingRules
                .Select(r => new EmailForwardingRuleDto(r.Id, r.Source, r.Destination, r.IsActive))
                .ToList(),
            domain.Reminders
                .Select(r => new DomainReminderDto(r.Id, r.ReminderType, r.SentTo, r.SentAt))
                .ToList());
    }
}
