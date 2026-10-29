using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence.Converters;

namespace Innovayse.Hostpanel.Integration.Tests.Persistence;

/// <summary>
/// Pins <see cref="SshProviderFailureConverter"/>: every failure has a stored form, the forms
/// written before the enum existed read back unchanged, and an unknown value never breaks a read.
/// </summary>
public sealed class SshProviderFailureConverterTests
{
    /// <summary>Every failure is stored and read back as itself.</summary>
    /// <param name="failure">The failure.</param>
    [Theory]
    [MemberData(nameof(AllFailures))]
    public void EveryFailure_RoundTrips(SshProviderFailure failure) =>
        Assert.Equal(failure, SshProviderFailureConverter.FromStored(SshProviderFailureConverter.ToStored(failure)));

    /// <summary>The codes already in the column keep their exact stored form.</summary>
    /// <param name="stored">The stored value.</param>
    /// <param name="failure">The failure it means.</param>
    [Theory]
    [InlineData("unreachable", SshProviderFailure.Unreachable)]
    [InlineData("timeout", SshProviderFailure.Timeout)]
    [InlineData("internal_error", SshProviderFailure.InternalError)]
    [InlineData("unsupported_version", SshProviderFailure.UnsupportedVersion)]
    [InlineData("duplicate_key", SshProviderFailure.DuplicateKey)]
    [InlineData("too_many_keys", SshProviderFailure.TooManyKeys)]
    [InlineData("ssh_disabled", SshProviderFailure.SshDisabled)]
    [InlineData("invalid_key", SshProviderFailure.InvalidKey)]
    [InlineData("account_not_found", SshProviderFailure.AccountNotFound)]
    [InlineData("account_suspended", SshProviderFailure.AccountSuspended)]
    [InlineData("agent_not_configured", SshProviderFailure.AgentNotConfigured)]
    public void ExistingStoredCodes_KeepTheirForm(string stored, SshProviderFailure failure)
    {
        Assert.Equal(failure, SshProviderFailureConverter.FromStored(stored));
        Assert.Equal(stored, SshProviderFailureConverter.ToStored(failure));
    }

    /// <summary>A stored value outside the table reads as <see cref="SshProviderFailure.Unknown"/>.</summary>
    [Fact]
    public void UnrecognisedStoredValue_ReadsAsUnknown() =>
        Assert.Equal(SshProviderFailure.Unknown, SshProviderFailureConverter.FromStored("no_such_code"));

    /// <summary>Every stored form fits the 64-character column.</summary>
    /// <param name="failure">The failure.</param>
    [Theory]
    [MemberData(nameof(AllFailures))]
    public void EveryStoredForm_FitsTheColumn(SshProviderFailure failure) =>
        Assert.InRange(SshProviderFailureConverter.ToStored(failure).Length, 1, 64);

    /// <summary>Every <see cref="SshProviderFailure"/> value.</summary>
    /// <returns>One row per value.</returns>
    public static TheoryData<SshProviderFailure> AllFailures()
    {
        var data = new TheoryData<SshProviderFailure>();
        foreach (var failure in Enum.GetValues<SshProviderFailure>())
        {
            data.Add(failure);
        }

        return data;
    }
}
