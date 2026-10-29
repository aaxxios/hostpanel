using Innovayse.Hostpanel.Domain.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Converters;

/// <summary>
/// Stores <see cref="EmailSendFailure"/> in <c>EmailLogs.Error</c> as the member name, keeping the
/// column type and length as they are.
/// </summary>
/// <remarks>
/// Rows written before the failure was a closed value hold the relay's free text; any value that is
/// not a member name reads back as <see cref="EmailSendFailure.Unknown"/>.
/// </remarks>
public sealed class EmailSendFailureConverter : ValueConverter<EmailSendFailure, string>
{
    /// <summary>Initialises the converter.</summary>
    public EmailSendFailureConverter()
        : base(failure => ToStored(failure), value => FromStored(value))
    {
    }

    /// <summary>Gets the stored form of a failure.</summary>
    /// <param name="failure">The failure to store.</param>
    /// <returns>The column value: the member name.</returns>
    public static string ToStored(EmailSendFailure failure) => failure.ToString();

    /// <summary>Reads a stored value back.</summary>
    /// <param name="value">The column value.</param>
    /// <returns>The failure, or <see cref="EmailSendFailure.Unknown"/> for anything that is not a defined member name.</returns>
    public static EmailSendFailure FromStored(string value) =>
        Enum.TryParse<EmailSendFailure>(value, ignoreCase: false, out var failure)
            && Enum.IsDefined(failure)
            && !int.TryParse(value, out _)
            ? failure
            : EmailSendFailure.Unknown;
}
