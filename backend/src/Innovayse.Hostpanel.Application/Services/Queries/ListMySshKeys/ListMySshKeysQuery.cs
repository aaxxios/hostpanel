using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Application.Services.Queries.ListMySshKeys;

/// <summary>Lists the SSH keys of one of the caller's own services.</summary>
/// <param name="ServiceId">The service, which must belong to the caller.</param>
public sealed record ListMySshKeysQuery(int ServiceId) : ICallerScopedServiceMessage;
