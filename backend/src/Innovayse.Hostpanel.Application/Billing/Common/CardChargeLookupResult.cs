using Innovayse.Hostpanel.Application.Billing.Common.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// A saved-method charge the card gateway already took for one payment attempt, or the answer that
/// no such attempt took any money.
/// </summary>
/// <param name="Found">
/// <see langword="true"/> only when the gateway holds a settled charge for the attempt; a decline,
/// or an attempt it never saw, is <see langword="false"/>.
/// </param>
/// <param name="TransactionId">The charge ID, or the intent ID when there is no charge; set when <paramref name="Found"/> is true.</param>
/// <param name="Amount">The amount taken, in the major currency unit; zero when nothing was found.</param>
/// <param name="Currency">ISO 4217 currency code of the amount taken; empty when nothing was found.</param>
/// <param name="Failure">
/// Why the gateway could not be asked, or <see langword="null"/> when it answered; a failed lookup
/// says nothing about the money, so it is never read as <see cref="None"/>.
/// </param>
public sealed record CardChargeLookupResult(
    bool Found,
    string? TransactionId,
    decimal Amount,
    string Currency,
    CardGatewayFailure? Failure = null)
{
    /// <summary>The answer for an attempt that took no money.</summary>
    public static CardChargeLookupResult None { get; } = new(false, null, 0m, string.Empty);

    /// <summary>Builds the answer for a lookup the gateway could not serve.</summary>
    /// <param name="failure">Why the gateway could not be asked.</param>
    /// <returns>A not-found result carrying <paramref name="failure"/>.</returns>
    public static CardChargeLookupResult Failed(CardGatewayFailure failure) => new(false, null, 0m, string.Empty, failure);
}
