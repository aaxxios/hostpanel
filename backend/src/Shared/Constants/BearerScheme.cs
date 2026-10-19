namespace Innovayse.Hostpanel.Shared.Constants;

/// <summary>The HTTP <c>Bearer</c> authorization scheme, spelled once for every layer that reads or writes the header value; the scheme name itself is <c>JwtBearerDefaults.AuthenticationScheme</c>.</summary>
public static class BearerScheme
{
    /// <summary>The scheme plus its separating space: what an <c>Authorization</c> header value starts with.</summary>
    public const string HeaderPrefix = "Bearer ";
}
