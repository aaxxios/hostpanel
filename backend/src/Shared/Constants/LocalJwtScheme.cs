namespace Innovayse.Hostpanel.Shared.Constants;

/// <summary>The authentication scheme name of the bearer handler for tokens this API issues itself.</summary>
public static class LocalJwtScheme
{
    /// <summary>The scheme name; the handler registration, the policies and the selector must agree on it.</summary>
    public const string Name = "LocalJwt";
}
