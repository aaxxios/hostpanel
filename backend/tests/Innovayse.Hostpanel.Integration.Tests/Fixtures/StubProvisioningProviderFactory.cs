using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;
/// <summary>
/// Test stub for <see cref="IProvisioningProviderFactory"/>. Hands back the same
/// <see cref="StubProvisioningProvider"/> for every server.
/// </summary>
/// <remarks>
/// Replacing <see cref="IProvisioningProvider"/> in the container was not enough on its
/// own: ProvisionServiceHandler does not resolve that interface, it asks this factory for
/// a provider built from the server row, and the real implementation always builds a hosting panel (v7)
/// client and calls the hostname on it. In a test that hostname belongs to a server that
/// does not exist, so provisioning failed at the network and the endpoint answered 400 —
/// the registered stub sitting unused beside it the whole time.
/// </remarks>
internal sealed class StubProvisioningProviderFactory : IProvisioningProviderFactory
{
    /// <summary>The single stub handed out for every server.</summary>
    private readonly StubProvisioningProvider _provider = new();

    /// <inheritdoc/>
    public IProvisioningProvider CreateFor(Server server) => _provider;
}
