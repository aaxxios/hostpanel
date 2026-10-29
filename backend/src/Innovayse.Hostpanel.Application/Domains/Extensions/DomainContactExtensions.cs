using Innovayse.Hostpanel.Domain.Domains;

namespace Innovayse.Hostpanel.Application.Domains.Extensions;

/// <summary>
/// The one rule for whether a registrant contact is complete enough to buy a domain with. Both
/// registrar adapters ask it before they send an order, so an incomplete contact is refused the
/// same way whichever registrar the name is bought at.
/// </summary>
public static class DomainContactExtensions
{
    /// <summary>
    /// Whether every detail a registrar requires of a registrant is present. State or province and
    /// the second address line are optional: not every country has one.
    /// </summary>
    /// <param name="contact">The registrant contact.</param>
    /// <returns><see langword="true"/> when the contact can be sent to a registrar.</returns>
    public static bool IsCompleteForPurchase(this DomainContact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        return !string.IsNullOrWhiteSpace(contact.FirstName)
            && !string.IsNullOrWhiteSpace(contact.LastName)
            && !string.IsNullOrWhiteSpace(contact.Email)
            && !string.IsNullOrWhiteSpace(contact.Phone)
            && !string.IsNullOrWhiteSpace(contact.Address1)
            && !string.IsNullOrWhiteSpace(contact.City)
            && !string.IsNullOrWhiteSpace(contact.PostalCode)
            && !string.IsNullOrWhiteSpace(contact.Country);
    }
}
