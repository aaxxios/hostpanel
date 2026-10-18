namespace Innovayse.Hostpanel.Domain.Orders;

/// <summary>
/// The domain actions an order line can carry. Persisted on the order item and sent by the
/// cart, so they stay lower-case strings.
/// </summary>
public static class DomainActionNames
{
    /// <summary>Register a new domain.</summary>
    public const string Register = "register";

    /// <summary>Transfer an existing domain in.</summary>
    public const string Transfer = "transfer";

    /// <summary>Renew a domain the client already holds.</summary>
    public const string Renew = "renew";
}
