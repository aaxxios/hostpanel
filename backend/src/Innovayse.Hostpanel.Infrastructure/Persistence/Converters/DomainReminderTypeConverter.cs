using Innovayse.Hostpanel.Domain.Domains.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Converters;

/// <summary>
/// EF Core value converter between <see cref="DomainReminderType"/> and the text stored in
/// <c>DomainReminders.ReminderType</c>.
/// </summary>
/// <remarks>
/// The stored texts are the persisted identifiers of each reminder type. They are kept exactly as
/// they were written before the column was typed, so every existing row keeps reading without a
/// schema or data change. Never rename a stored text here; that would orphan the rows holding it.
/// </remarks>
public sealed class DomainReminderTypeConverter : ValueConverter<DomainReminderType, string>
{
    /// <summary>Stored text of <see cref="DomainReminderType.ThirtyDaysBeforeExpiry"/>.</summary>
    private const string ThirtyDaysBeforeExpiry = "30 Days Before Expiry";

    /// <summary>Stored text of <see cref="DomainReminderType.FourteenDaysBeforeExpiry"/>.</summary>
    private const string FourteenDaysBeforeExpiry = "14 Days Before Expiry";

    /// <summary>Stored text of <see cref="DomainReminderType.SevenDaysBeforeExpiry"/>.</summary>
    private const string SevenDaysBeforeExpiry = "7 Days Before Expiry";

    /// <summary>Stored text of <see cref="DomainReminderType.OneDayBeforeExpiry"/>.</summary>
    private const string OneDayBeforeExpiry = "1 Days Before Expiry";

    /// <summary>Stored text of <see cref="DomainReminderType.ExpiredNotice"/>.</summary>
    private const string ExpiredNotice = "Expired Notice";

    /// <summary>Stored text of <see cref="DomainReminderType.RedemptionNotice"/>.</summary>
    private const string RedemptionNotice = "Redemption Notice";

    /// <summary>Initialises the converter with the fixed enum to stored text mapping.</summary>
    public DomainReminderTypeConverter()
        : base(type => ToStored(type), stored => FromStored(stored))
    {
    }

    /// <summary>Returns the stored text of a reminder type.</summary>
    /// <param name="type">The reminder type.</param>
    /// <returns>The text persisted for <paramref name="type"/>.</returns>
    private static string ToStored(DomainReminderType type)
    {
        return type switch
        {
            DomainReminderType.ThirtyDaysBeforeExpiry => ThirtyDaysBeforeExpiry,
            DomainReminderType.FourteenDaysBeforeExpiry => FourteenDaysBeforeExpiry,
            DomainReminderType.SevenDaysBeforeExpiry => SevenDaysBeforeExpiry,
            DomainReminderType.OneDayBeforeExpiry => OneDayBeforeExpiry,
            DomainReminderType.ExpiredNotice => ExpiredNotice,
            DomainReminderType.RedemptionNotice => RedemptionNotice,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown domain reminder type."),
        };
    }

    /// <summary>Returns the reminder type a stored text identifies.</summary>
    /// <param name="stored">The text read from the column.</param>
    /// <returns>
    /// The matching <see cref="DomainReminderType"/>, or <see cref="DomainReminderType.Unknown"/> for a
    /// text this version does not recognise, so such a row never breaks a read.
    /// </returns>
    private static DomainReminderType FromStored(string stored)
    {
        return stored switch
        {
            ThirtyDaysBeforeExpiry => DomainReminderType.ThirtyDaysBeforeExpiry,
            FourteenDaysBeforeExpiry => DomainReminderType.FourteenDaysBeforeExpiry,
            SevenDaysBeforeExpiry => DomainReminderType.SevenDaysBeforeExpiry,
            OneDayBeforeExpiry => DomainReminderType.OneDayBeforeExpiry,
            ExpiredNotice => DomainReminderType.ExpiredNotice,
            RedemptionNotice => DomainReminderType.RedemptionNotice,
            _ => DomainReminderType.Unknown,
        };
    }
}
