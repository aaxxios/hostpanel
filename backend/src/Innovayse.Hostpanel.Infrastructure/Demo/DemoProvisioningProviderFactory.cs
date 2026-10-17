using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>Hands out the demo provisioning provider for any server.</summary>
internal sealed class DemoProvisioningProviderFactory : IProvisioningProviderFactory
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
    public IProvisioningProvider CreateFor(Server server) => _provider;
}
