namespace Innovayse.Hostpanel.Shared.Authorization;

/// <summary>
/// The authorization policy names every controller action names in <c>[Authorize(Policy = …)]</c>.
/// Registered once, in <c>Program.cs</c>, over hostpanel's roles.
/// </summary>
/// <remarks>
/// hostpanel has no organizations, so the reference set's <c>OrgAdmin</c>/<c>PlatformAdmin</c> have no
/// meaning here; the roles it does have are the staff <c>Admin</c>, the <c>Reseller</c> and the
/// <c>Client</c> (conformance spec §2).
/// </remarks>
public static class Policies
{
    /// <summary>Any signed-in caller, whatever their role.</summary>
    public const string Member = "Member";

    /// <summary>Staff: the admin panel's operators.</summary>
    public const string Admin = "Admin";

    /// <summary>A reseller acting on its own clients, or staff.</summary>
    public const string ResellerOrAdmin = "ResellerOrAdmin";

    /// <summary>A client of the portal acting on its own account.</summary>
    public const string Client = "Client";

    /// <summary>A service-to-service caller on an <c>/internal/*</c> route, authenticated by service key.</summary>
    public const string InternalService = "InternalService";
}
