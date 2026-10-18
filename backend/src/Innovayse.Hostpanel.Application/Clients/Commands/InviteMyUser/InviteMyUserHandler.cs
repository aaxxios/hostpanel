using Innovayse.Hostpanel.Application.Clients.Commands.InviteUserToClient;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Clients.Commands.InviteMyUser;

/// <summary>
/// Turns the portal's permission keys into the bit-flags value <see cref="InviteUserToClientCommand"/>
/// takes, then dispatches it.
/// </summary>
public sealed class InviteMyUserHandler
{
    /// <summary>The permissions value that grants every permission, compared case-insensitively.</summary>
    private const string AllPermissionsKeyword = "all";

    /// <summary>
    /// The permission keys the client portal sends, in the order it lists them.
    /// </summary>
    private static readonly Dictionary<string, ClientPermission> PermissionKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["profile"] = ClientPermission.ModifyMasterProfile,
        ["contacts"] = ClientPermission.ViewManageContacts,
        ["products"] = ClientPermission.ViewProductsServices,
        ["passwords"] = ClientPermission.ViewModifyPasswords,
        ["sso"] = ClientPermission.AllowSingleSignOn,
        ["domains"] = ClientPermission.ViewDomains,
        ["domainsettings"] = ClientPermission.ManageDomainSettings,
        ["invoices"] = ClientPermission.ViewPayInvoices,
        ["quotes"] = ClientPermission.ViewAcceptQuotes,
        ["tickets"] = ClientPermission.ViewOpenSupportTickets,
        ["affiliate"] = ClientPermission.ViewManageAffiliate,
        ["emails"] = ClientPermission.ViewEmails,
        ["orders"] = ClientPermission.PlaceNewOrders,
    };

    /// <summary>Wolverine bus, for dispatching the underlying invite command.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus, for dispatching the underlying invite command.</param>
    public InviteMyUserHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Invites someone to the caller's own account.</summary>
    /// <param name="cmd">The invite command, carrying the portal's permission keys.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record, otherwise whatever <see cref="InviteUserToClientCommand"/> answers with -- see
    /// <see cref="InviteUserToClientHandler.HandleAsync"/>.
    /// </returns>
    public async Task<Result> HandleAsync(InviteMyUserCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var profile = await _bus.GetCallerClientAsync(ct);
        if (profile.IsFailure)
        {
            return Result.FailureCode(profile.ErrorCode!);
        }

        var permissions = ParsePermissions(cmd.Permissions);
        return await _bus.InvokeAsync<Result>(
            new InviteUserToClientCommand(profile.Value!.Id, cmd.Email, cmd.FirstName, cmd.LastName, (int)permissions), ct);
    }

    /// <summary>Turns the portal's permission keys into the stored bit-flags value.</summary>
    /// <param name="permissions">
    /// The literal <c>all</c>, a comma-separated list of keys from <see cref="PermissionKeys"/>, or null.
    /// </param>
    /// <returns>The permissions to grant; none when nothing recognisable was sent.</returns>
    /// <remarks>
    /// Unknown keys are ignored rather than rejected, and the reason is which way the mistake
    /// falls: dropping one grants less than was asked for, while refusing the whole request over a
    /// single stale key would block an invitation the sender can do nothing about.
    /// </remarks>
    private static ClientPermission ParsePermissions(string? permissions)
    {
        if (string.IsNullOrWhiteSpace(permissions))
        {
            return ClientPermission.None;
        }

        if (permissions.Trim().Equals(AllPermissionsKeyword, StringComparison.OrdinalIgnoreCase))
        {
            return ClientPermission.All;
        }

        var granted = ClientPermission.None;
        foreach (var key in permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (PermissionKeys.TryGetValue(key, out var permission))
            {
                granted |= permission;
            }
        }

        return granted;
    }
}
