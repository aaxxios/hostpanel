using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;

/// <summary>
/// Checks Name.am credentials by logging in with the values saved on the admin Integrations page.
/// </summary>
public sealed class NameAmConnectionProbe : IIntegrationConnectionProbe
{
    /// <summary>The Name.am API client whose login is exercised.</summary>
    private readonly NameAmClient _client;

    /// <summary>Initializes the probe.</summary>
    /// <param name="client">Name.am API client.</param>
    public NameAmConnectionProbe(NameAmClient client)
    {
        _client = client;
    }

    /// <inheritdoc/>
    public string Slug => IntegrationSlugs.NameAm;

    /// <inheritdoc/>
    public Task ProbeAsync(CancellationToken ct) => _client.VerifyCredentialsAsync(ct);
}
