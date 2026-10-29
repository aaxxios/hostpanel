using Innovayse.Hostpanel.Application.Clients.Common;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetClientBriefs;

/// <summary>
/// Requests a <see cref="ClientBriefDto"/> for each named client record, so another module can show
/// or notify a client without reading the Clients tables itself. Records are named by id, by the
/// owner's identity subject, or both; a key that names no record is left out of the answer.
/// </summary>
/// <param name="ClientIds">The client record ids to describe, or <see langword="null"/> for none.</param>
/// <param name="UserSubjects">The owners' identity subjects to describe, or <see langword="null"/> for none.</param>
/// <param name="WithEmail">
/// <see langword="true"/> to resolve each owner's sign-in address and language too, which costs one identity
/// lookup for the whole batch; list screens that show only a name leave it off.
/// </param>
public sealed record GetClientBriefsQuery(
    IReadOnlyList<int>? ClientIds = null,
    IReadOnlyList<string>? UserSubjects = null,
    bool WithEmail = false);
