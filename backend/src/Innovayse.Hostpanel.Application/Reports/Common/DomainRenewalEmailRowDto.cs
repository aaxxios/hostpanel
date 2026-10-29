using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One domain renewal reminder email row.</summary>
/// <param name="ClientName">The client name.</param>
/// <param name="Domain">The domain.</param>
/// <param name="SentAt">The sent at.</param>
/// <param name="Reminder">Kind of reminder sent, serialised by name.</param>
/// <param name="Recipients">The recipients.</param>
public record DomainRenewalEmailRowDto(
    string ClientName,
    string Domain,
    DateTimeOffset SentAt,
    DomainReminderType Reminder,
    string Recipients);
