using Innovayse.SDK.Plugins;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A configurable <see cref="IHostingPanelApiClient"/> standing in for the real hosting panel control-panel API a
/// test must not call over the network. Succeeds with fixed values by default; setting
/// <see cref="ShouldThrow"/> makes it throw, the way an unreachable panel would.
/// </summary>
public sealed class FakeHostingPanelApiClient : IHostingPanelApiClient
{
    /// <summary>When <see langword="true"/>, <see cref="GetServerInfoAsync"/> throws instead of answering.</summary>
    public bool ShouldThrow { get; set; }

    /// <summary>The account count returned when <see cref="ShouldThrow"/> is <see langword="false"/>.</summary>
    public int AccountsCount { get; set; } = 5;

    /// <summary>The version string returned when <see cref="ShouldThrow"/> is <see langword="false"/>.</summary>
    public string CwpVersion { get; set; } = "1.9.1";

    /// <inheritdoc/>
    public Task<(int AccountsCount, string CwpVersion)> GetServerInfoAsync(
        string host, string port, string apiKey, CancellationToken ct)
    {
        if (ShouldThrow)
        {
            throw new HttpRequestException(
                "fake hosting panel server refused the request with connection details nobody should see");
        }

        return Task.FromResult((AccountsCount, CwpVersion));
    }
}
