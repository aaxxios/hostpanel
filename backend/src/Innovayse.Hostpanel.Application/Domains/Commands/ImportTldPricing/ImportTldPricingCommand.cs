namespace Innovayse.Hostpanel.Application.Domains.Commands.ImportTldPricing;

/// <summary>Command to bulk-import TLD pricing from a registrar provider.</summary>
/// <param name="Module">The registrar module to import from, named as a <see cref="Innovayse.Hostpanel.Domain.Domains.Enums.RegistrarModule"/> member.</param>
public sealed record ImportTldPricingCommand(string Module);
