namespace Innovayse.Hostpanel.Shared.Constants;

/// <summary>
///     Standard JWT claim names used across all microservices.
///     Matches <c>JwtRegisteredClaimNames</c> but avoids dependency
///     on <c>System.IdentityModel.Tokens.Jwt</c> in every project.
/// </summary>
public static class JwtClaims
{
    /// <summary>Subject — user ID.</summary>
    public const string Sub = "sub";

    /// <summary>Email address.</summary>
    public const string Email = "email";

    /// <summary>Preferred language/locale (en, ru, hy).</summary>
    public const string Locale = "locale";

    /// <summary>User role.</summary>
    public const string Role = "role";

    /// <summary>Display name (full name or email fallback).</summary>
    public const string Name = "name";

    /// <summary>The SSO organization id — the tenant every row is scoped to.</summary>
    public const string OrgId = "org_id";

    /// <summary>The caller's role in the organization: member, admin or owner.</summary>
    public const string OrgRole = "org_role";

    /// <summary>The app ids the caller may open.</summary>
    public const string Apps = "apps";

    /// <summary>The token's space-separated scopes.</summary>
    public const string Scope = "scope";

    /// <summary>Whether the caller is a platform administrator.</summary>
    public const string IsSuperadmin = "is_superadmin";
}
