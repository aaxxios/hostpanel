using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Servers;

/// <summary>Unit tests for <see cref="ServerFillPolicy"/>.</summary>
public sealed class ServerFillPolicyTests
{
    /// <summary>The fixed moment every test acts at.</summary>
    private static readonly DateTimeOffset Now = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    /// <summary>The server with the lowest fill ratio wins.</summary>
    [Fact]
    public void SelectBest_PicksTheLeastFilledServer()
    {
        var busy = NewServer("busy", maxAccounts: 10, accounts: 8);
        var quiet = NewServer("quiet", maxAccounts: 10, accounts: 2);

        ServerFillPolicy.SelectBest([busy, quiet]).Should().BeSameAs(quiet);
    }

    /// <summary>On an equal ratio the server with the larger capacity wins.</summary>
    [Fact]
    public void SelectBest_BreaksARatioTieByCapacity()
    {
        var small = NewServer("small", maxAccounts: 10, accounts: 5);
        var large = NewServer("large", maxAccounts: 100, accounts: 50);

        ServerFillPolicy.SelectBest([small, large]).Should().BeSameAs(large);
    }

    /// <summary>A tie on both keys keeps the order the candidates were given in.</summary>
    [Fact]
    public void SelectBest_KeepsTheGivenOrderOnAFullTie()
    {
        var first = NewServer("a", maxAccounts: 10, accounts: 1);
        var second = NewServer("b", maxAccounts: 10, accounts: 1);

        ServerFillPolicy.SelectBest([first, second]).Should().BeSameAs(first);
    }

    /// <summary>Disabled and full servers are never chosen.</summary>
    [Fact]
    public void SelectBest_SkipsDisabledAndFullServers()
    {
        var disabled = NewServer("off", maxAccounts: 10, accounts: 0, isDisabled: true);
        var full = NewServer("full", maxAccounts: 10, accounts: 10);
        var open = NewServer("open", maxAccounts: 10, accounts: 9);

        ServerFillPolicy.SelectBest([disabled, full, open]).Should().BeSameAs(open);
    }

    /// <summary>No eligible candidate gives no server.</summary>
    [Fact]
    public void SelectBest_ReturnsNullWhenNothingQualifies()
    {
        var full = NewServer("full", maxAccounts: 3, accounts: 3);

        ServerFillPolicy.SelectBest([full]).Should().BeNull();
        ServerFillPolicy.SelectBest([]).Should().BeNull();
    }

    /// <summary>A server without a limit counts as empty and is preferred over a partly filled one.</summary>
    [Fact]
    public void SelectBest_TreatsAnUnlimitedServerAsEmpty()
    {
        var limited = NewServer("limited", maxAccounts: 10, accounts: 1);
        var unlimited = NewServer("unlimited", maxAccounts: null, accounts: 500);

        ServerFillPolicy.SelectBest([limited, unlimited]).Should().BeSameAs(unlimited);
        ServerFillPolicy.IsFull(unlimited).Should().BeFalse();
        ServerFillPolicy.FillRatio(unlimited).Should().Be(0.0);
    }

    /// <summary>An unknown account count counts as zero accounts.</summary>
    [Fact]
    public void FillRatio_TreatsAnUnknownCountAsEmpty()
    {
        var unknown = NewServer("unknown", maxAccounts: 10, accounts: null);

        ServerFillPolicy.FillRatio(unknown).Should().Be(0.0);
        ServerFillPolicy.IsFull(unknown).Should().BeFalse();
    }

    /// <summary>The ratio is accounts over the limit.</summary>
    [Fact]
    public void FillRatio_DividesAccountsByTheLimit()
    {
        var server = NewServer("half", maxAccounts: 4, accounts: 1);

        ServerFillPolicy.FillRatio(server).Should().Be(0.25);
    }

    /// <summary>Builds a server with the given capacity and account count.</summary>
    /// <param name="name">Server name.</param>
    /// <param name="maxAccounts">Account limit, or null for none.</param>
    /// <param name="accounts">Accounts reported by the last connection test, or null when unknown.</param>
    /// <param name="isDisabled">Whether the server is disabled.</param>
    /// <returns>The server.</returns>
    private static Server NewServer(string name, int? maxAccounts, int? accounts, bool isDisabled = false)
    {
        var server = Server.Create(new ServerDetails(
            Name: name,
            Hostname: $"{name}.example.test",
            IpAddress: null,
            AssignedIpAddresses: null,
            Module: ServerModule.Cwp7,
            Username: "api",
            Credentials: new ServerCredentials(null, null, null),
            UseSSL: true,
            MaxAccounts: maxAccounts,
            IsDefault: false,
            IsDisabled: isDisabled,
            MonthlyCost: 0m,
            Datacenter: null,
            ServerStatusAddress: null,
            NameServers: ServerNameServers.None), Now);
        server.RecordConnectionTest(isOnline: true, accountsCount: accounts, new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero));
        return server;
    }
}
