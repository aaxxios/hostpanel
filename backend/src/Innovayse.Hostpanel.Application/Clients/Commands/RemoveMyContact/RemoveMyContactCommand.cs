namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveMyContact;

/// <summary>
/// Command for the authenticated client to remove a contact from their own account.
/// </summary>
/// <remarks>Carries no client id: it is resolved inside the handler from the credential.</remarks>
/// <param name="ContactId">The contact's primary key.</param>
public sealed record RemoveMyContactCommand(int ContactId);
