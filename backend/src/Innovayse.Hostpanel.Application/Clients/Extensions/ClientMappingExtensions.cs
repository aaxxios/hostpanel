using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Domain.Clients;

namespace Innovayse.Hostpanel.Application.Clients.Extensions;

/// <summary>
/// Maps a <see cref="Client"/> aggregate to <see cref="ClientDto"/>. The admin read of any client
/// and the client's own profile read go through this one mapping, so a field added to the DTO is
/// added once rather than in two handlers that quietly drift apart.
/// </summary>
public static class ClientMappingExtensions
{
    /// <summary>Maps a <see cref="Client"/> aggregate, with its contacts loaded, to <see cref="ClientDto"/>.</summary>
    /// <param name="client">The client aggregate to map; its contacts must be loaded.</param>
    /// <param name="email">The email from the Identity user.</param>
    /// <param name="twoFactorEnabled">Whether TOTP 2FA is enabled for the user.</param>
    /// <param name="language">The account's chosen UI language, null where the provider holds none.</param>
    /// <returns>The mapped DTO.</returns>
    public static ClientDto ToDto(this Client client, string email, bool twoFactorEnabled, string? language) =>
        new(
            client.Id,
            client.UserId,
            email,
            client.FirstName,
            client.LastName,
            client.CompanyName,
            client.Phone,
            client.Status,
            client.Street,
            client.Address2,
            client.City,
            client.State,
            client.PostCode,
            client.Country,
            // From the account, not the client row: the language belongs to the person who signs
            // in, and this product's own table is the only store it ever lived in.
            language,
            client.Currency,
            client.PaymentMethod,
            client.BillingContact,
            client.AdminNotes,
            client.NotifyGeneral,
            client.NotifyInvoice,
            client.NotifySupport,
            client.NotifyProduct,
            client.NotifyDomain,
            client.NotifyAffiliate,
            client.LateFees,
            client.OverdueNotices,
            client.TaxExempt,
            client.SeparateInvoices,
            client.DisableCcProcessing,
            client.MarketingOptIn,
            client.StatusUpdate,
            client.AllowSso,
            twoFactorEnabled,
            client.CreatedAt,
            client.Contacts.Select(c => new ContactDto(
                c.Id, c.FirstName, c.LastName, c.CompanyName,
                c.Email, c.Phone, c.Type,
                c.Street, c.Address2, c.City, c.State, c.PostCode, c.Country,
                c.NotifyGeneral, c.NotifyInvoice, c.NotifySupport,
                c.NotifyProduct, c.NotifyDomain, c.NotifyAffiliate)).ToList());
}
