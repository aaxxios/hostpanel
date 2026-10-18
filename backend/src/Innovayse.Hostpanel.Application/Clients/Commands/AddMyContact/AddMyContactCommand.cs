using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Commands.AddMyContact;

/// <summary>
/// Command for the authenticated client to add a contact to their own account.
/// </summary>
/// <remarks>
/// Carries no client id: it is resolved inside the handler from the credential, which is what
/// stops one account adding a contact -- and with it a notification address -- to somebody else's.
/// </remarks>
/// <param name="FirstName">The contact's first name.</param>
/// <param name="LastName">The contact's last name.</param>
/// <param name="CompanyName">Optional company name.</param>
/// <param name="Email">The contact's email address.</param>
/// <param name="Phone">Optional phone number.</param>
/// <param name="Type">Contact type classification.</param>
/// <param name="Street">Optional street address.</param>
/// <param name="Address2">Optional second address line.</param>
/// <param name="City">Optional city.</param>
/// <param name="State">Optional state or region.</param>
/// <param name="PostCode">Optional postal code.</param>
/// <param name="Country">Optional ISO 3166-1 alpha-2 country code.</param>
/// <param name="NotifyGeneral">General email notifications.</param>
/// <param name="NotifyInvoice">Invoice email notifications.</param>
/// <param name="NotifySupport">Support email notifications.</param>
/// <param name="NotifyProduct">Product email notifications.</param>
/// <param name="NotifyDomain">Domain email notifications.</param>
/// <param name="NotifyAffiliate">Affiliate email notifications.</param>
public sealed record AddMyContactCommand(
    string FirstName,
    string LastName,
    string? CompanyName,
    string Email,
    string? Phone,
    ContactType Type,
    string? Street,
    string? Address2,
    string? City,
    string? State,
    string? PostCode,
    string? Country,
    bool NotifyGeneral,
    bool NotifyInvoice,
    bool NotifySupport,
    bool NotifyProduct,
    bool NotifyDomain,
    bool NotifyAffiliate);
