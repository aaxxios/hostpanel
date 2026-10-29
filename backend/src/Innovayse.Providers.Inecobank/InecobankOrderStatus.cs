namespace Innovayse.Providers.Inecobank;

/// <summary>Parsed getOrderStatusExtended.do response.</summary>
/// <param name="ErrorCode">Gateway errorCode (0 = request processed; does NOT mean paid).</param>
/// <param name="OrderStatus">Gateway orderStatus (2 = deposited); null when absent.</param>
/// <param name="ErrorMessage">Gateway errorMessage when present.</param>
/// <param name="AuthRefNum">Bank reference number when present (used as the transaction id).</param>
/// <param name="Amount">Order amount in minor units when present.</param>
/// <param name="Currency">ISO 4217 numeric currency code of the order when present.</param>
public sealed record InecobankOrderStatus(
    int ErrorCode, int? OrderStatus, string? ErrorMessage, string? AuthRefNum, long? Amount, string? Currency);
