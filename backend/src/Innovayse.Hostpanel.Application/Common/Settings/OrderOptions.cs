using System.Globalization;
using Innovayse.Hostpanel.Domain.Orders;

namespace Innovayse.Hostpanel.Application.Common.Settings;

/// <summary>
/// How a placed order is numbered and how long its invoice stays open. Bound from the "Orders"
/// section; a deployment sets these only to change the defaults.
/// </summary>
public sealed class OrderOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Orders";

    /// <summary>Text put in front of every order number, for example <c>ORD-</c>.</summary>
    public string NumberPrefix { get; set; } = "ORD-";

    /// <summary>Minimum number of digits of the sequence part; shorter numbers are zero-padded.</summary>
    public int NumberDigits { get; set; } = 4;

    /// <summary>Days an order's invoice stays open before it falls due.</summary>
    public int InvoicePaymentTermDays { get; set; } = 7;

    /// <summary>
    /// Whether the values can number an order and date its invoice: at least one digit, a prefix
    /// and padding that together fit <see cref="Order.OrderNumberMaxLength"/>, and a payment term
    /// of 0 to 365 days.
    /// </summary>
    public bool IsValid =>
        NumberPrefix is not null
        && NumberDigits >= 1
        && NumberPrefix.Length + NumberDigits <= Order.OrderNumberMaxLength
        && InvoicePaymentTermDays is >= 0 and <= 365;

    /// <summary>How long an order's invoice stays open.</summary>
    public TimeSpan InvoicePaymentTerm => TimeSpan.FromDays(InvoicePaymentTermDays);

    /// <summary>Formats a sequence value as an order number.</summary>
    /// <param name="sequence">The next value of the order number sequence.</param>
    /// <returns>The prefix followed by the zero-padded sequence value, for example <c>ORD-0042</c>.</returns>
    public string FormatNumber(long sequence) =>
        NumberPrefix + sequence.ToString(new string('0', NumberDigits), CultureInfo.InvariantCulture);

    /// <summary>
    /// Whether a sequence value still formats into an order number the order table can store.
    /// </summary>
    /// <remarks>
    /// <see cref="NumberDigits"/> is only the padding: a sequence that outgrows it formats longer,
    /// so <see cref="IsValid"/> cannot settle this at startup and the value is checked when it is
    /// handed out.
    /// </remarks>
    /// <param name="sequence">The next value of the order number sequence.</param>
    /// <returns><see langword="true"/> when the formatted number fits <see cref="Order.OrderNumberMaxLength"/>.</returns>
    public bool Fits(long sequence) => FormatNumber(sequence).Length <= Order.OrderNumberMaxLength;
}
