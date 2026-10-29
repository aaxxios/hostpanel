using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>DTO for an additional client contact.</summary>
/// <param name="Id">Contact primary key.</param>
/// <param name="FirstName">Contact first name.</param>
/// <param name="LastName">Contact last name.</param>
/// <param name="CompanyName">Optional company name.</param>
/// <param name="Email">Contact email address.</param>
/// <param name="Phone">Contact phone number (null if not set).</param>
/// <param name="Type">Contact type classification.</param>
/// <param name="Address">The contact's postal address.</param>
/// <param name="Notifications">Which kinds of email the contact receives.</param>
public sealed record ContactDto(
    int Id,
    string FirstName,
    string LastName,
    string? CompanyName,
    string Email,
    string? Phone,
    ContactType Type,
    PostalAddress Address,
    ContactNotifications Notifications);
