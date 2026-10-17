namespace Innovayse.Hostpanel.Domain.Orders.Enums;
/// <summary>Lifecycle status of an order.</summary>
public enum OrderStatus
{
    /// <summary>Order placed, awaiting admin review.</summary>
    Pending = 0,

    /// <summary>Order accepted by admin, services created.</summary>
    Active = 1,

    /// <summary>Order cancelled by admin or client.</summary>
    Cancelled = 2,

    /// <summary>Order flagged as fraudulent.</summary>
    Fraud = 3,
}
