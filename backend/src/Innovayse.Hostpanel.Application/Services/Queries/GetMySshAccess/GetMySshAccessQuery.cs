using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetMySshAccess;

/// <summary>Asks for the SSH access summary of one of the caller's own services.</summary>
/// <param name="ServiceId">The service, which must belong to the caller.</param>
public sealed record GetMySshAccessQuery(int ServiceId) : ICallerScopedServiceMessage;
