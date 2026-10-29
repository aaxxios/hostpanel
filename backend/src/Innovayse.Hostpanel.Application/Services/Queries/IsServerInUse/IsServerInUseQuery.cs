namespace Innovayse.Hostpanel.Application.Services.Queries.IsServerInUse;

/// <summary>
/// Asks whether a server still hosts anything that belongs to the Services module: a client service
/// or an SSH key. The server module asks it before it deletes a server, so it never reads those tables.
/// </summary>
/// <param name="ServerId">The server to check.</param>
public sealed record IsServerInUseQuery(int ServerId);
