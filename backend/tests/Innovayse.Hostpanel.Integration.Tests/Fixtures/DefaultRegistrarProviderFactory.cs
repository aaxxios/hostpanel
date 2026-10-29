using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Infrastructure.Registrar;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// The registrar factory of the test host. In the running API the default registrar provider and the
/// provider of the default module are one and the same (both are Name.am), so a domain registered at
/// the default stores that module and every later operation reaches the same provider. The test host
/// swaps the default provider for a stub; this factory answers the default module with whatever
/// provider is registered as the default, and every other module with the production factory, so
/// that pairing holds in the tests too. Both arrive through the constructor, so the test host registers it
/// by type and Wolverine's generated handler code builds it like the production factory.
/// </summary>
public sealed class DefaultRegistrarProviderFactory : IRegistrarProviderFactory
{
    /// <summary>The module the default provider belongs to in the running API.</summary>
    private const RegistrarModule DefaultModule = RegistrarModule.NameAm;

    /// <summary>The provider registered as the default.</summary>
    private readonly IRegistrarProvider _defaultProvider;

    /// <summary>The production factory, for every other module.</summary>
    private readonly RegistrarProviderFactory _modules;

    /// <summary>Initializes the factory.</summary>
    /// <param name="defaultProvider">The provider registered as the default.</param>
    /// <param name="modules">The production factory, for every other module.</param>
    public DefaultRegistrarProviderFactory(IRegistrarProvider defaultProvider, RegistrarProviderFactory modules)
    {
        _defaultProvider = defaultProvider;
        _modules = modules;
    }

    /// <inheritdoc/>
    public IRegistrarProvider GetProvider(RegistrarModule module) =>
        module == DefaultModule ? _defaultProvider : _modules.GetProvider(module);
}
