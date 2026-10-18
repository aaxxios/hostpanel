namespace Innovayse.Hostpanel.Domain.Domains;

/// <summary>
/// The values of <see cref="RegisteredDomain.OrderType"/>, persisted as capitalised strings.
/// </summary>
public static class DomainOrderTypes
{
    /// <summary>The domain was registered through the platform.</summary>
    public const string Register = "Register";

    /// <summary>The domain was transferred in.</summary>
    public const string Transfer = "Transfer";
}
