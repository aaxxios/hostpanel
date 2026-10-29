namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.DeleteServerGroup;

/// <summary>
/// Deletes a server group. Servers in the group are unassigned, not deleted.
/// </summary>
/// <param name="Id">The group to delete.</param>
public sealed record DeleteServerGroupCommand(int Id);
