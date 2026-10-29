using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>The <c>profileData</c> section of a client data export: who the account belongs to.</summary>
/// <param name="Id">The client's primary key.</param>
/// <param name="FirstName">Given name on the account.</param>
/// <param name="LastName">Family name on the account.</param>
/// <param name="Email">Sign-in address, read from the identity store rather than the client row.</param>
/// <param name="CompanyName">Company the account bills as, when it is not a person.</param>
/// <param name="Phone">Contact telephone number.</param>
/// <param name="Address">Billing address.</param>
/// <param name="Status">Account status: Active, Inactive, Suspended or Closed.</param>
/// <param name="Billing">Currency, credit, payment method and billing contact.</param>
/// <param name="MarketingOptIn">Whether the client agreed to marketing emails.</param>
/// <param name="Notifications">Which kinds of email the account receives.</param>
/// <param name="Settings">The account switches an admin set.</param>
/// <param name="Registration">What was captured about the device and network the registration came from, when it was recorded.</param>
/// <param name="CreatedAt">When the client record was opened.</param>
public sealed record ClientExportProfileDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? CompanyName,
    string? Phone,
    PostalAddress Address,
    ClientStatus Status,
    ClientExportBillingDto Billing,
    bool MarketingOptIn,
    ClientExportNotificationsDto Notifications,
    ClientExportSettingsDto Settings,
    RegistrationInfo Registration,
    DateTimeOffset CreatedAt);
