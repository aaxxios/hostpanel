using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;
using Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap;

namespace Innovayse.Hostpanel.Infrastructure.Registrar;

/// <summary>
/// Resolves <see cref="IRegistrarProvider"/> implementations by <see cref="RegistrarModule"/>.
/// Each provider arrives through the constructor with its own dependencies (HTTP client, settings),
/// so the factory never reaches into the container and Wolverine's generated handler code can build
/// it as a plain constructor call. Both providers are cheap to create: the HTTP client each one holds
/// comes from the shared client factory and nothing is called until a method is.
/// </summary>
public sealed class RegistrarProviderFactory : IRegistrarProviderFactory
{
    /// <summary>The provider for domains held at <see cref="RegistrarModule.NameAm"/>.</summary>
    private readonly NameAmRegistrarProvider _nameAm;

    /// <summary>The provider for domains held at <see cref="RegistrarModule.Namecheap"/>.</summary>
    private readonly NamecheapRegistrarProvider _namecheap;

    /// <summary>Initializes a new instance of the <see cref="RegistrarProviderFactory"/> class.</summary>
    /// <param name="nameAm">The provider for domains held at <see cref="RegistrarModule.NameAm"/>.</param>
    /// <param name="namecheap">The provider for domains held at <see cref="RegistrarModule.Namecheap"/>.</param>
    public RegistrarProviderFactory(NameAmRegistrarProvider nameAm, NamecheapRegistrarProvider namecheap)
    {
        _nameAm = nameAm;
        _namecheap = namecheap;
    }

    /// <summary>
    /// Returns the <see cref="IRegistrarProvider"/> implementation for the specified registrar module.
    /// </summary>
    /// <param name="module">The registrar module whose provider to resolve.</param>
    /// <returns>The corresponding <see cref="IRegistrarProvider"/> implementation.</returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="module"/> has no registered provider.</exception>
    public IRegistrarProvider GetProvider(RegistrarModule module)
    {
        return module switch
        {
            RegistrarModule.NameAm => _nameAm,
            RegistrarModule.Namecheap => _namecheap,
            _ => throw new NotSupportedException($"No registrar provider for module '{module}'."),
        };
    }
}
