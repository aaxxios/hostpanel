namespace Innovayse.Hostpanel.Application.Services.Queries.ListOverdueSuspendedServices;

/// <summary>
/// Lists the ids of the services suspended for an overdue invoice, so billing can lift those no overdue
/// invoice covers any more.
/// </summary>
public sealed record ListOverdueSuspendedServicesQuery;
