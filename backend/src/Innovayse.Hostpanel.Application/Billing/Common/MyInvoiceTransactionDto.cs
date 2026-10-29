using Innovayse.Hostpanel.Domain.Billing.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// One money movement on the client's own invoice, as the client area reads it. The gateway's
/// own reference, its fee and the staff notes of <see cref="InvoiceTransactionDto"/> are not
/// part of it.
/// </summary>
/// <param name="Id">Transaction primary key.</param>
/// <param name="Date">UTC timestamp of the transaction.</param>
/// <param name="Amount">Transaction amount, always positive; <paramref name="Type"/> says which way the money moved.</param>
/// <param name="Type">Transaction type (Payment, Refund, Credit).</param>
public sealed record MyInvoiceTransactionDto(
    int Id,
    DateTimeOffset Date,
    decimal Amount,
    InvoiceTransactionType Type);
