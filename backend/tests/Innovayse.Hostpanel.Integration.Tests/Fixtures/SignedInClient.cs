namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A registered, signed-in client user with the client account it owns, its identity subject and
/// the email address it registered with.
/// </summary>
/// <param name="Client">The HTTP client carrying the user's bearer token.</param>
/// <param name="ClientId">The id of the client account the user owns.</param>
/// <param name="UserId">The user's identity subject.</param>
/// <param name="Email">The email address the user registered with.</param>
public sealed record SignedInClient(HttpClient Client, int ClientId, string UserId, string Email)
{
    /// <summary>Deconstructs into the signed-in HTTP client and the owned client id.</summary>
    /// <param name="client">The HTTP client carrying the user's bearer token.</param>
    /// <param name="clientId">The id of the client account the user owns.</param>
    public void Deconstruct(out HttpClient client, out int clientId)
    {
        client = Client;
        clientId = ClientId;
    }

    /// <summary>Deconstructs into the signed-in HTTP client, the owned client id and the identity subject.</summary>
    /// <param name="client">The HTTP client carrying the user's bearer token.</param>
    /// <param name="clientId">The id of the client account the user owns.</param>
    /// <param name="userId">The user's identity subject.</param>
    public void Deconstruct(out HttpClient client, out int clientId, out string userId)
    {
        client = Client;
        clientId = ClientId;
        userId = UserId;
    }

    /// <summary>Deconstructs into the signed-in HTTP client, the owned client id, the identity subject and the registered email.</summary>
    /// <param name="client">The HTTP client carrying the user's bearer token.</param>
    /// <param name="clientId">The id of the client account the user owns.</param>
    /// <param name="userId">The user's identity subject.</param>
    /// <param name="email">The email address the user registered with.</param>
    public void Deconstruct(out HttpClient client, out int clientId, out string userId, out string email)
    {
        client = Client;
        clientId = ClientId;
        userId = UserId;
        email = Email;
    }
}
