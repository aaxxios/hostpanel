namespace Innovayse.Hostpanel.Domain.Orders.Enums;
/// <summary>Lifecycle status of an individual order item.</summary>
public enum OrderItemStatus
{
    /// <summary>Item pending, not yet fulfilled.</summary>
    Pending = 0,

    /// <summary>Item fulfilled, service created.</summary>
    Active = 1,

    /// <summary>Item cancelled.</summary>
    Cancelled = 2,
}
