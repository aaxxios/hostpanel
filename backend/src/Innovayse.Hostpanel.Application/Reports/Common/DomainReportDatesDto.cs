namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>The dates of a domain row in the Domains report, each <c>yyyy-MM-dd</c>.</summary>
/// <param name="Registration">The registration date.</param>
/// <param name="Expiry">The expiry date.</param>
/// <param name="NextDue">The next due date.</param>
public sealed record DomainReportDatesDto(string Registration, string Expiry, string NextDue);
