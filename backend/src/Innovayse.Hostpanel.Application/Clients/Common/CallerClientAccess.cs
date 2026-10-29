using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>
/// The client account the caller acts for, and what the caller may do on it.
/// </summary>
/// <param name="ClientId">
/// The account the caller acts for: the record they own, or the one record they are linked to.
/// <see langword="null"/> when they own none and are linked to none, and also when they are linked
/// to several, because nothing on the request says which account is meant.
/// </param>
/// <param name="IsOwner"><see langword="true"/> when <paramref name="ClientId"/> is the record the caller owns.</param>
/// <param name="Permissions">
/// <see cref="ClientPermission.All"/> for the owner; otherwise what the link grants (what every link
/// grants when there are several), limited to the flags this product defines.
/// </param>
public sealed record CallerClientAccess(int? ClientId, bool IsOwner, ClientPermission Permissions)
{
    /// <summary>Gets the access of a caller who owns no account, is linked to none, or is not identified at all.</summary>
    public static CallerClientAccess None { get; } = new(null, false, ClientPermission.None);
}
