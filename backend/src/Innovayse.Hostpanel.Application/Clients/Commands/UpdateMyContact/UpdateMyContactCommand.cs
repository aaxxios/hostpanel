using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateMyContact;

/// <summary>
/// Command for the authenticated client to update a contact on their own account.
/// </summary>
/// <remarks>
/// Carries no client id: it is resolved inside the handler from the credential.
/// </remarks>
/// <param name="ContactId">The contact's primary key; the route supplies it.</param>
/// <param name="FirstName">The contact's first name.</param>
/// <param name="LastName">The contact's last name.</param>
/// <param name="CompanyName">Optional company name.</param>
/// <param name="Email">The contact's email address.</param>
/// <param name="Phone">Optional phone number.</param>
/// <param name="Type">Contact type classification.</param>
/// <param name="Address">The contact's postal address; every line is optional and the country is an ISO 3166-1 alpha-2 code.</param>
/// <param name="Notifications">Which kinds of email the contact receives.</param>
public sealed record UpdateMyContactCommand(
    int ContactId,
    string FirstName,
    string LastName,
    string? CompanyName,
    string Email,
    string? Phone,
    ContactType Type,
    PostalAddress Address,
    ContactNotifications Notifications);
