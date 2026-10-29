using Innovayse.Hostpanel.Domain.Orders.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Converters;

/// <summary>
/// Stores a <see cref="DomainAction"/> in <c>OrderItems.DomainAction</c> as the lower-case text the
/// column has always held.
/// </summary>
/// <remarks>
/// The texts are kept exactly as they were written before the column was typed, so every existing row
/// keeps reading without a data change. Never rename a stored text here; that would orphan the rows
/// holding it.
/// </remarks>
public sealed class DomainActionConverter : ValueConverter<DomainAction, string>
{
    /// <summary>Stored text of <see cref="DomainAction.Register"/>.</summary>
    private const string Register = "register";

    /// <summary>Stored text of <see cref="DomainAction.Transfer"/>.</summary>
    private const string Transfer = "transfer";

    /// <summary>Stored text of <see cref="DomainAction.Renew"/>.</summary>
    private const string Renew = "renew";

    /// <summary>Initialises the converter.</summary>
    public DomainActionConverter()
        : base(action => ToStored(action), value => FromStored(value))
    {
    }

    /// <summary>Gets the stored form of an action.</summary>
    /// <param name="action">The domain action to store.</param>
    /// <returns>The column value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a defined action.</exception>
    public static string ToStored(DomainAction action) => action switch
    {
        DomainAction.Register => Register,
        DomainAction.Transfer => Transfer,
        DomainAction.Renew => Renew,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown domain action."),
    };

    /// <summary>Reads a stored value back.</summary>
    /// <param name="value">The column value.</param>
    /// <returns>The action the text names.</returns>
    /// <exception cref="InvalidOperationException">The text names no domain action.</exception>
    public static DomainAction FromStored(string value) => value switch
    {
        Register => DomainAction.Register,
        Transfer => DomainAction.Transfer,
        Renew => DomainAction.Renew,
        _ => throw new InvalidOperationException($"'{value}' is not a domain action."),
    };
}
