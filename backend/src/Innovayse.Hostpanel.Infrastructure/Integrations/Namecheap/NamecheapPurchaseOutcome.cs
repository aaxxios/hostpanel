using System.Xml.Linq;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap;

/// <summary>
/// The outcome of a Namecheap command that buys something: either the response to read, or the
/// reason to hand back when Namecheap answered <c>ERROR</c> and nothing was bought.
/// </summary>
/// <param name="Response">The response document, or <see langword="null"/> when refused.</param>
/// <param name="RefusalCode">
/// The <c>REGISTRAR_*</c> error code for the refusal, or <see langword="null"/> when the command went through.
/// </param>
internal sealed record NamecheapPurchaseOutcome(XDocument? Response, string? RefusalCode);
