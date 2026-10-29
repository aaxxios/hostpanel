using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>DTO for a domain expiry reminder record.</summary>
/// <param name="Id">Reminder primary key.</param>
/// <param name="ReminderType">Kind of reminder sent, serialised by name.</param>
/// <param name="SentTo">Recipient email address.</param>
/// <param name="SentAt">UTC timestamp when the reminder was sent.</param>
public record DomainReminderDto(int Id, DomainReminderType ReminderType, string SentTo, DateTimeOffset SentAt);
