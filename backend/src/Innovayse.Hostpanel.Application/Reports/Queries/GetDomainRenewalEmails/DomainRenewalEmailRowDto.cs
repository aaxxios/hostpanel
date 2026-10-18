namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDomainRenewalEmails;

/// <summary>One domain renewal reminder email row.</summary>
/// <param name="ClientName">The client name.</param>
/// <param name="Domain">The domain.</param>
/// <param name="SentAt">The sent at.</param>
/// <param name="Reminder">The reminder.</param>
/// <param name="Recipients">The recipients.</param>
public record DomainRenewalEmailRowDto(
    string ClientName,
    string Domain,
    DateTimeOffset SentAt,
    string Reminder,
    string Recipients);
