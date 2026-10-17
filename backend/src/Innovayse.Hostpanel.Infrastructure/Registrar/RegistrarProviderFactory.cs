using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;
using Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap;
using Microsoft.Extensions.DependencyInjection;
namespace Innovayse.Hostpanel.Infrastructure.Registrar;
/// <summary>
/// Resolves <see cref="IRegistrarProvider"/> implementations by <see cref="RegistrarModule"/>.
/// Delegates to the DI container so each provider receives its own dependencies (HTTP clients, settings, etc.).
/// </summary>
public sealed class RegistrarProviderFactory : IRegistrarProviderFactory
{
    /// <summary>The scoped service provider for resolving concrete provider types.</summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Initializes a new instance of the <see cref="RegistrarProviderFactory"/> class.</summary>
    /// <param name="serviceProvider">The scoped service provider for resolving concrete provider types.</param>
    public RegistrarProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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
            RegistrarModule.NameAm => _serviceProvider.GetRequiredService<NameAmRegistrarProvider>(),
            RegistrarModule.Namecheap => _serviceProvider.GetRequiredService<NamecheapRegistrarProvider>(),
            _ => throw new NotSupportedException($"No registrar provider for module '{module}'."),
        };
    }
}
