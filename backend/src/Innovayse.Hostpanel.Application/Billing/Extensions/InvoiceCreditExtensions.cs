using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Billing.Extensions;

/// <summary>
/// Moves credit between an invoice and its client's balance, so the two always change in the same
/// save and credit is never created or lost on the way. The invoice decides the amount; the client's
/// balance is only reached through <see cref="IClientCredit"/>.
/// </summary>
public static class InvoiceCreditExtensions
{
    /// <summary>Takes credit from the client's balance and applies it to the invoice.</summary>
    /// <param name="clientCredit">The client credit port, staging on the caller's unit of work.</param>
    /// <param name="invoice">The invoice (tracked).</param>
    /// <param name="amount">Credit to move, in the invoice currency.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_NOT_FOUND</c> / <c>CLIENT_CREDIT_INSUFFICIENT</c> from the balance; the
    /// caller must not save on a refusal.
    /// </returns>
    /// <exception cref="Innovayse.Hostpanel.Domain.DomainRuleException">The invoice no longer takes credit, or owes less than <paramref name="amount"/>.</exception>
    public static async Task<Result> ApplyFromBalanceAsync(
        this IClientCredit clientCredit, Invoice invoice, decimal amount, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(clientCredit);
        ArgumentNullException.ThrowIfNull(invoice);

        // Invoice first: its refusal (status, more than owed) throws before the balance is touched.
        invoice.ApplyCredit(amount);
        var taken = await clientCredit.DeductAsync(new ClientCreditChange(invoice.ClientId, amount), ct);
        return taken.IsFailure ? Result.FailureCode(taken.ErrorCode!) : Result.Success();
    }

    /// <summary>Takes credit off the invoice, never more than it holds, and returns it to the client's balance.</summary>
    /// <param name="clientCredit">The client credit port, staging on the caller's unit of work.</param>
    /// <param name="invoice">The invoice (tracked).</param>
    /// <param name="amount">Credit to take back, in the invoice currency.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>CLIENT_NOT_FOUND</c> when the invoice's client is gone; the caller must not save then.</returns>
    public static async Task<Result> ReturnToBalanceAsync(
        this IClientCredit clientCredit, Invoice invoice, decimal amount, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(clientCredit);
        ArgumentNullException.ThrowIfNull(invoice);

        var removed = invoice.RemoveCredit(amount);
        if (removed <= 0)
        {
            return Result.Success();
        }

        var returned = await clientCredit.AddAsync(new ClientCreditChange(invoice.ClientId, removed), ct);
        return returned.IsFailure ? Result.FailureCode(returned.ErrorCode!) : Result.Success();
    }
}
