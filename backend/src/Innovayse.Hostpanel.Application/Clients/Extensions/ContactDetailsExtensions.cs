using Innovayse.Hostpanel.Application.Clients.Commands.AddContact;
using Innovayse.Hostpanel.Application.Clients.Commands.UpdateContact;
using Innovayse.Hostpanel.Domain.Clients;

namespace Innovayse.Hostpanel.Application.Clients.Extensions;

/// <summary>Turns the contact commands into the typed input the <see cref="Client"/> aggregate takes.</summary>
public static class ContactDetailsExtensions
{
    /// <summary>Builds the contact input of an add command.</summary>
    /// <param name="cmd">The command.</param>
    /// <returns>The contact input.</returns>
    public static ContactDetails ToDetails(this AddContactCommand cmd) => new(
        cmd.FirstName,
        cmd.LastName,
        cmd.CompanyName,
        cmd.Email,
        cmd.Phone,
        cmd.Type,
        cmd.Address,
        cmd.Notifications);

    /// <summary>Builds the contact input of an update command.</summary>
    /// <param name="cmd">The command.</param>
    /// <returns>The contact input.</returns>
    public static ContactDetails ToDetails(this UpdateContactCommand cmd) => new(
        cmd.FirstName,
        cmd.LastName,
        cmd.CompanyName,
        cmd.Email,
        cmd.Phone,
        cmd.Type,
        cmd.Address,
        cmd.Notifications);
}
