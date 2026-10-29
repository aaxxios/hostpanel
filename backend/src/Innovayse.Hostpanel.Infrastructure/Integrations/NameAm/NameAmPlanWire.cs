using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;

/// <summary>The plan a Name.am purchase item is bought on.</summary>
/// <param name="Id">
/// The plan id: the term and the behaviour joined as <c>{years}_year_{behaviour}</c>, for example
/// <c>2_year_register</c>, <c>1_year_renew</c> or <c>1_year_transfer</c>. The behaviour is the
/// <c>behavior</c> a product's plan reports in <c>GET /client/products</c>.
/// </param>
internal sealed record NameAmPlanWire([property: JsonPropertyName("_id")] string Id);
