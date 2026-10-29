using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Commands.AddUserToClient;

/// <summary>Command to add a non-owner user to a client account with specified permissions.</summary>
/// <param name="ClientId">The client account primary key.</param>
/// <param name="MemberUserId">The Identity user ID to link.</param>
/// <param name="Permissions">Granted permissions as flags; the wire also accepts the numeric bit mask.</param>
public sealed record AddUserToClientCommand(int ClientId, string MemberUserId, ClientPermission Permissions);
