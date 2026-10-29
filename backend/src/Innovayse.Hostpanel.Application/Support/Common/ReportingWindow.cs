namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>The date boundaries a reporting period covers.</summary>
/// <param name="From">The inclusive start.</param>
/// <param name="To">The end.</param>
internal sealed record ReportingWindow(DateTimeOffset From, DateTimeOffset To);
