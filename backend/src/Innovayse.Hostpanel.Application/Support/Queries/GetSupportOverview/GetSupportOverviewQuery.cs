using Innovayse.Hostpanel.Application.Support.Common.Enums;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetSupportOverview;

/// <summary>Query to retrieve support overview dashboard statistics for a given period.</summary>
/// <param name="Period">Time period filter; defaults to today when omitted.</param>
public sealed record GetSupportOverviewQuery(SupportOverviewPeriod Period = SupportOverviewPeriod.Today);
