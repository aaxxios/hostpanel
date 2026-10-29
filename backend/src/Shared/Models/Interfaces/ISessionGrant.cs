namespace Innovayse.Hostpanel.Shared.Models.Interfaces;

/// <summary>
/// A sign-in answer that may hand the browser a session. Implemented by the DTOs a sign-in use case
/// returns so <c>BaseApiController.HandleSession</c> can set the httpOnly session cookie from a
/// <see cref="Result{T}"/> without the controller touching the token or the response.
/// </summary>
public interface ISessionGrant
{
    /// <summary>
    /// The access token to place in the httpOnly session cookie, or <see langword="null"/> when this
    /// answer opens no session yet (a second factor is still owed). Implemented explicitly so the
    /// wire body of a DTO is not changed by it.
    /// </summary>
    public string? SessionToken { get; }
}
