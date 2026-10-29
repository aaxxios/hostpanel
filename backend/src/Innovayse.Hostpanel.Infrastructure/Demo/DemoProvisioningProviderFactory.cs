using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>Hands out the demo provisioning provider for any server.</summary>
public sealed class DemoProvisioningProviderFactory : IProvisioningProviderFactory
{
    /// <summary>The one fake provider every server resolves to.</summary>
    private readonly IProvisioningProvider _provider;

    /// <summary>Creates the factory.</summary>
    /// <param name="provider">The one fake provider every server resolves to.</param>
    public DemoProvisioningProviderFactory(IProvisioningProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc/>
    public IProvisioningProvider CreateFor(ServerConnectionDto server) => _provider;
}
