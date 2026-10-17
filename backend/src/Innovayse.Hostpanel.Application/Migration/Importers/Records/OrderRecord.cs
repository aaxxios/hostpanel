namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One order as the source plugin describes it, with its line items.</summary>
/// <param name="ClientEmail">The client email.</param>
/// <param name="OrderNumber">The order number.</param>
/// <param name="PaymentMethod">The payment method.</param>
/// <param name="Status">The status.</param>
/// <param name="Amount">The amount.</param>
/// <param name="CreatedAt">The created at.</param>
/// <param name="Items">The items entries.</param>
internal sealed record OrderRecord(string ClientEmail, string OrderNumber, string PaymentMethod, string Status, decimal Amount, DateTimeOffset CreatedAt, List<OrderItemRecord>? Items);
