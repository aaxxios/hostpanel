namespace Innovayse.Hostpanel.Application.Clients.Queries.GetMyContacts;

/// <summary>
/// Query for the additional contacts on the authenticated client's own account.
/// </summary>
/// <remarks>Carries no client id: whose account is resolved inside the handler from the credential.</remarks>
public sealed record GetMyContactsQuery();
