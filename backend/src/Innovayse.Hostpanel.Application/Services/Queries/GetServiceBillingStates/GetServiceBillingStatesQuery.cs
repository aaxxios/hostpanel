namespace Innovayse.Hostpanel.Application.Services.Queries.GetServiceBillingStates;

/// <summary>Reads the suspension-related state of a set of services, for the billing module.</summary>
/// <param name="ServiceIds">The services; an id that names no service is left out of the answer.</param>
public sealed record GetServiceBillingStatesQuery(IReadOnlyList<int> ServiceIds);
