namespace Innovayse.Hostpanel.Application.Services.Queries.ListOverdueSuspensionsPastLimit;

/// <summary>
/// Lists the ids of the services suspended for non-payment at or before <paramref name="SuspendedBy"/>
/// that still renew automatically, so the daily termination run can end them.
/// </summary>
/// <param name="SuspendedBy">The latest suspension moment that is past the limit; the caller's moment minus the allowed days.</param>
public sealed record ListOverdueSuspensionsPastLimitQuery(DateTimeOffset SuspendedBy);
