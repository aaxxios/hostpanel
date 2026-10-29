namespace Innovayse.Hostpanel.Domain.Clients.Enums;

/// <summary>Lifecycle status of a client account.</summary>
public enum ClientStatus
{
    /// <summary>Account is active and can use services.</summary>
    Active = 0,

    /// <summary>Account is inactive (not yet activated or manually deactivated).</summary>
    Inactive = 1,

    /// <summary>Account is suspended due to non-payment or policy violation.</summary>
    Suspended = 2,

    /// <summary>Account is permanently closed.</summary>
    Closed = 3,
}
