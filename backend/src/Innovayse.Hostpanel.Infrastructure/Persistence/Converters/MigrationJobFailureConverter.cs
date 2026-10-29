using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Converters;

/// <summary>
/// Stores <see cref="MigrationJobFailure"/> in <c>MigrationJobs.ErrorMessage</c> as the member name,
/// keeping the column type and length as they are.
/// </summary>
/// <remarks>
/// Rows written before the failure was a closed value hold free text; any value that is not a member
/// name reads back as <see cref="MigrationJobFailure.Unknown"/>.
/// </remarks>
public sealed class MigrationJobFailureConverter : ValueConverter<MigrationJobFailure, string>
{
    /// <summary>Initialises the converter.</summary>
    public MigrationJobFailureConverter()
        : base(failure => ToStored(failure), value => FromStored(value))
    {
    }

    /// <summary>Gets the stored form of a failure.</summary>
    /// <param name="failure">The failure to store.</param>
    /// <returns>The column value: the member name.</returns>
    public static string ToStored(MigrationJobFailure failure) => failure.ToString();

    /// <summary>Reads a stored value back.</summary>
    /// <param name="value">The column value.</param>
    /// <returns>The failure, or <see cref="MigrationJobFailure.Unknown"/> for anything that is not a defined member name.</returns>
    public static MigrationJobFailure FromStored(string value) =>
        Enum.TryParse<MigrationJobFailure>(value, ignoreCase: false, out var failure)
            && Enum.IsDefined(failure)
            && !int.TryParse(value, out _)
            ? failure
            : MigrationJobFailure.Unknown;
}
