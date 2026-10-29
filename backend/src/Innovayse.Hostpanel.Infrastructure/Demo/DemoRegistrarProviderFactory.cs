using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>Hands out the demo registrar whichever registrar module is asked for.</summary>
public sealed class DemoRegistrarProviderFactory : IRegistrarProviderFactory
{
    /// <summary>The one fake registrar every module resolves to.</summary>
    private readonly IRegistrarProvider _provider;

    /// <summary>Creates the factory.</summary>
    /// <param name="provider">The one fake registrar every module resolves to.</param>
    public DemoRegistrarProviderFactory(IRegistrarProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc/>
    public IRegistrarProvider GetProvider(RegistrarModule module) => _provider;
}
