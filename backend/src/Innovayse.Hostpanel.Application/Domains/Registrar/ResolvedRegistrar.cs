using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Application.Domains.Registrar;

/// <summary>
/// The registrar a TLD resolves to: the provider to call and the module that names it, so a new
/// domain can store the module it was placed at.
/// </summary>
/// <param name="Provider">The registrar provider.</param>
/// <param name="Module">The module the provider belongs to.</param>
public sealed record ResolvedRegistrar(IRegistrarProvider Provider, RegistrarModule Module);
