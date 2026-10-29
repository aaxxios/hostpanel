using Innovayse.Hostpanel.Domain.Notifications.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence.Converters;

namespace Innovayse.Hostpanel.Integration.Tests.Persistence;

/// <summary>
/// Tests for <see cref="EmailSendFailureConverter"/>: a reason is stored as its name, and a row
/// written before the reason was a closed value reads back as <see cref="EmailSendFailure.Unknown"/>.
/// </summary>
public sealed class EmailSendFailureConverterTests
{
    /// <summary>Every reason survives the round trip through its stored name.</summary>
    [Fact]
    public void EveryReason_RoundTripsThroughItsName()
    {
        foreach (var failure in Enum.GetValues<EmailSendFailure>())
        {
            var stored = EmailSendFailureConverter.ToStored(failure);

            Assert.Equal(failure.ToString(), stored);
            Assert.Equal(failure, EmailSendFailureConverter.FromStored(stored));
        }
    }

    /// <summary>The relay's free text from an older row, or a bare number, reads as Unknown.</summary>
    /// <param name="stored">The column value.</param>
    [Theory]
    [InlineData("550 5.1.1 Mailbox unavailable.")]
    [InlineData("4")]
    [InlineData("authenticationrejected")]
    public void LegacyOrForeignValue_ReadsAsUnknown(string stored)
    {
        Assert.Equal(EmailSendFailure.Unknown, EmailSendFailureConverter.FromStored(stored));
    }
}
