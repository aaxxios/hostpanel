namespace Innovayse.Hostpanel.Domain.Clients;

/// <summary>
/// Refusal codes the <see cref="Client"/> aggregate throws as
/// <see cref="Innovayse.Hostpanel.Domain.DomainRuleException"/>.
/// </summary>
/// <remarks>
/// Declared here, not in <c>Application.Common.Errors.HostpanelErrorCodes</c>, because Domain may
/// not reference Application; <c>HostpanelErrorCodes</c> re-declares the same string values from
/// these constants so <c>ResourceCoverageTests</c> (which only reflects over
/// <c>HostpanelErrorCodes</c>) still requires a sentence for each one in every language.
/// </remarks>
public static class ClientsErrors
{
    /// <summary>The id names no contact on this client (404) — <see cref="Client.UpdateContact"/> and removal.</summary>
    public const string ClientContactNotFound = "CLIENT_CONTACT_NOT_FOUND";

    /// <summary>The account owner cannot also be added as an additional user (400) — <see cref="Client.AddUser"/>.</summary>
    public const string ClientUserIsOwner = "CLIENT_USER_IS_OWNER";

    /// <summary>The user is already linked to this client (400) — <see cref="Client.AddUser"/>.</summary>
    public const string ClientUserLinkDuplicate = "CLIENT_USER_LINK_DUPLICATE";

    /// <summary>
    /// The user is not linked to this client (400) — <see cref="Client.RemoveUser"/> and
    /// <see cref="Client.UpdateUserPermissions"/>.
    /// </summary>
    public const string ClientUserNotLinked = "CLIENT_USER_NOT_LINKED";

    /// <summary>The named user is already the account owner (400) — <see cref="Client.TransferOwnership"/>.</summary>
    public const string ClientTransferSameOwner = "CLIENT_TRANSFER_SAME_OWNER";

    /// <summary>The deduction exceeds the client's credit balance (400) — <see cref="Client.DeductCredit"/>.</summary>
    public const string ClientCreditInsufficient = "CLIENT_CREDIT_INSUFFICIENT";

    /// <summary>
    /// The signed-in user is not allowed to do this on the account: the permission the action needs
    /// was not granted, or the user has no relationship to any account (403). Raised by the client
    /// portal's permission filter, not by the aggregate; declared here with the other client codes.
    /// </summary>
    public const string ClientPermissionDenied = "CLIENT_PERMISSION_DENIED";

    /// <summary>The account is not in a state that can be suspended (400) — <see cref="Client.Suspend"/>.</summary>
    public const string ClientSuspendNotAllowed = "CLIENT_SUSPEND_NOT_ALLOWED";

    /// <summary>The account is already closed (400) — <see cref="Client.Close"/>.</summary>
    public const string ClientCloseNotAllowed = "CLIENT_CLOSE_NOT_ALLOWED";

    /// <summary>A closed account cannot be reactivated this way (400) — <see cref="Client.Activate"/>.</summary>
    public const string ClientActivateNotAllowed = "CLIENT_ACTIVATE_NOT_ALLOWED";
}
