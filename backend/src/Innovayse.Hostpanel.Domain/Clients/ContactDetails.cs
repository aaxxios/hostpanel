using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Domain.Clients;

/// <summary>
/// Everything an admin or the account holder enters about one additional contact: the input of
/// <see cref="Client.AddContact"/> and <see cref="Client.UpdateContact"/>.
/// </summary>
/// <param name="FirstName">The contact's first name; not blank.</param>
/// <param name="LastName">The contact's last name; not blank.</param>
/// <param name="CompanyName">The optional company name.</param>
/// <param name="Email">The contact's email address; unique among the client's contacts, compared without regard to case.</param>
/// <param name="Phone">The optional phone number.</param>
/// <param name="Type">The contact type classification.</param>
/// <param name="Address">The contact's postal address.</param>
/// <param name="Notifications">Which emails the contact receives.</param>
public sealed record ContactDetails(
    string FirstName,
    string LastName,
    string? CompanyName,
    string Email,
    string? Phone,
    ContactType Type,
    PostalAddress Address,
    ContactNotifications Notifications);
