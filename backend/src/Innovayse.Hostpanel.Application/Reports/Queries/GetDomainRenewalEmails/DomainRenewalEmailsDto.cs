namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDomainRenewalEmails;

/// <summary>Domain renewal reminder emails report.</summary>
/// <param name="Rows">The rows entries.</param>
public record DomainRenewalEmailsDto(IReadOnlyList<DomainRenewalEmailRowDto> Rows);
