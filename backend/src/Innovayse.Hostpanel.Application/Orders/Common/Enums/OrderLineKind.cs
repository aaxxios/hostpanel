namespace Innovayse.Hostpanel.Application.Orders.Common.Enums;

/// <summary>What fulfilling an order line produces.</summary>
public enum OrderLineKind
{
    /// <summary>A product line, fulfilled by creating a client service.</summary>
    Service = 0,

    /// <summary>A domain line, fulfilled by registering, transferring or renewing a domain.</summary>
    Domain = 1,
}
