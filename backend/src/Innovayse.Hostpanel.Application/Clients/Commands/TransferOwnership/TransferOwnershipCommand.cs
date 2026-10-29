namespace Innovayse.Hostpanel.Application.Clients.Commands.TransferOwnership;

/// <summary>Command to transfer account ownership from the current owner to a different user.</summary>
/// <param name="ClientId">The client account primary key.</param>
/// <param name="NewOwnerUserId">The Identity user ID of the new owner.</param>
public sealed record TransferOwnershipCommand(int ClientId, string NewOwnerUserId);
