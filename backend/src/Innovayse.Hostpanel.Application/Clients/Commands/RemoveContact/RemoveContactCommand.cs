namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveContact;

/// <summary>
/// Command to remove a contact from a client account.
/// </summary>
/// <param name="ClientId">The owning client's primary key.</param>
/// <param name="ContactId">The contact's primary key.</param>
public sealed record RemoveContactCommand(int ClientId, int ContactId);
