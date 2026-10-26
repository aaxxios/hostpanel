using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// What the signed-in user may do on a client account, decided from stored data and the identity of
/// the request only.
/// </summary>
/// <remarks>
/// <para>
/// The client portal's permission filter asks this port; nothing else decides whether a portal action
/// is allowed. Like <c>IInvoiceOwnership</c> and <c>IDomainOwnership</c> it resolves the caller itself
/// through <c>ICurrentUserService</c>: the caller is never named by the request, so a body or a route
/// cannot talk its way into somebody else's permissions.
/// </para>
/// <para>
/// It fails closed. A caller who owns no client record and is linked to none gets
/// <see cref="ClientPermission.None"/>, and so does a caller the request does not identify.
/// </para>
/// </remarks>
public interface IClientAccess
{
    /// <summary>Reads the permissions the current credential holds on a client account.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see cref="ClientPermission.All"/> for the owner of a client record; otherwise the
    /// permissions the caller's stored link grants, limited to the flags this product defines (a
    /// stored bit nobody defined grants nothing). When the caller is linked to several accounts the
    /// result is only what every link grants, because nothing on the request says which account is
    /// meant and a refusal is the safe reading of that doubt. <see cref="ClientPermission.None"/>
    /// when the caller owns no record, is linked to none, or is not identified at all.
    /// </returns>
    Task<ClientPermission> GetCallerPermissionsAsync(CancellationToken ct);
}
