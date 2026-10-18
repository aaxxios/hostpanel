using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Provisioning;
using Innovayse.Hostpanel.Infrastructure.Integrations.CPanel.Settings;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.CPanel;
/// <summary>
/// cPanel WHM implementation of <see cref="IProvisioningProvider"/>.
/// Delegates all provisioning operations to <see cref="CPanelClient"/>.
/// </summary>
public sealed class CPanelProvisioningProvider : IProvisioningProvider
{
    /// <summary>The client this class uses.</summary>
    private readonly CPanelClient _client;

    /// <summary>Initializes a new instance of the <see cref="CPanelProvisioningProvider"/> class.</summary>
    /// <param name="client">The client this class uses.</param>
    /// <param name="options">The options this class uses.</param>
    public CPanelProvisioningProvider(CPanelClient client, IOptions<CPanelOptions> options)
    {
        _client = client;
        _settings = options.Value;
    }

    /// <summary>Resolved cPanel configuration settings.</summary>
    private readonly CPanelOptions _settings;

    /// <inheritdoc/>
    public async Task<ProvisioningResult> ProvisionAsync(ProvisionAccountOrder req, CancellationToken ct)
    {
        await _client.CreateAccountAsync(
            req.DomainName,
            req.Username,
            req.Password,
            req.Package,
            ct);

        return new ProvisioningResult(true, req.Username, null);
    }

    /// <inheritdoc/>
    public async Task SuspendAsync(SuspendAccountOrder req, CancellationToken ct)
    {
        await _client.SuspendAccountAsync(req.ProvisioningRef, req.Reason, ct);
    }

    /// <inheritdoc/>
    public async Task UnsuspendAsync(string provisioningRef, CancellationToken ct)
    {
        await _client.UnsuspendAccountAsync(provisioningRef, ct);
    }

    /// <inheritdoc/>
    public async Task TerminateAsync(TerminateAccountOrder req, CancellationToken ct)
    {
        await _client.RemoveAccountAsync(req.ProvisioningRef, ct);
    }

    /// <inheritdoc/>
    public Task<ServiceCredentials> GetCredentialsAsync(string provisioningRef, CancellationToken ct)
    {
        // cPanel does not expose stored passwords via the API.
        // Return the username as provisioningRef with a placeholder password.
        var credentials = new ServiceCredentials(
            provisioningRef,
            "**hidden**",
            $"{provisioningRef}.hosted",
            _settings.ServerIp,
            $"{_settings.ApiUrl.TrimEnd('/')}/cpanel");

        return Task.FromResult(credentials);
    }

    /// <inheritdoc/>
    public Task<string> GetCPanelSsoUrlAsync(string provisioningRef, CancellationToken ct)
    {
        return _client.GetCPanelSsoUrlAsync(provisioningRef, ct);
    }

    /// <inheritdoc/>
    public async Task ChangePasswordAsync(string provisioningRef, string newPassword, CancellationToken ct)
    {
        await _client.ChangePasswordAsync(provisioningRef, newPassword, ct);
    }

    /// <inheritdoc/>
    public async Task ChangePackageAsync(string provisioningRef, string newPackage, CancellationToken ct)
    {
        await _client.ChangePackageAsync(provisioningRef, newPackage, ct);
    }
}
