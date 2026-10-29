namespace Innovayse.Hostpanel.Application.Products.Queries.IsServerGroupInUse;

/// <summary>
/// Asks whether a product still provisions into a server group. The server module asks it before it
/// deletes a group, so it never reads the Products tables.
/// </summary>
/// <param name="ServerGroupId">The server group to check.</param>
public sealed record IsServerGroupInUseQuery(int ServerGroupId);
