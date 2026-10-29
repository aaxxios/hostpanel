using Innovayse.Hostpanel.Application.Admin.Servers.Common;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerBriefs;

/// <summary>
/// Requests a <see cref="ServerBriefDto"/> for each named server, so another module can show where
/// a service runs without reading the Servers table itself.
/// </summary>
/// <param name="ServerIds">The servers to describe; an id that names no server is left out of the answer.</param>
public sealed record GetServerBriefsQuery(IReadOnlyList<int> ServerIds);
