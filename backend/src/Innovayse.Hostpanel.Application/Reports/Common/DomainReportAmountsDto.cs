namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>The money figures of a domain row in the Domains report.</summary>
/// <param name="FirstPayment">The first payment amount.</param>
/// <param name="Recurring">The recurring amount.</param>
public sealed record DomainReportAmountsDto(decimal FirstPayment, decimal Recurring);
