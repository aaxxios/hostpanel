namespace Innovayse.Hostpanel.Domain.Orders.Enums;

/// <summary>What an order line does to a domain.</summary>
public enum DomainAction
{
    /// <summary>Register a new domain.</summary>
    Register = 0,

    /// <summary>Transfer an existing domain in.</summary>
    Transfer = 1,

    /// <summary>Renew a domain the client already holds.</summary>
    Renew = 2,
}
