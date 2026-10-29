using Innovayse.Hostpanel.Application.Admin.Integrations.Common;
using Innovayse.Hostpanel.Domain.Settings;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Integrations;

/// <summary>Pins the single integration catalogue the list, detail and connection-test use cases read.</summary>
public sealed class IntegrationCatalogueTests
{
    /// <summary>Every field a catalogue entry needs for configuration is a field its form actually renders.</summary>
    [Fact]
    public void EveryRequiredFieldIsAFormField()
    {
        foreach (var (slug, entry) in IntegrationCatalogue.All)
        {
            var formKeys = entry.Fields.Select(f => f.Key).ToHashSet();

            entry.RequiredFields.Should().BeSubsetOf(formKeys, $"{slug} must only require fields it renders");
        }
    }

    /// <summary>The hosted gateway is not a built-in: it is listed through the plugin registry.</summary>
    [Fact]
    public void HostedGatewayIsNotBuiltIn()
    {
        IntegrationCatalogue.TryGet(IntegrationSlugs.Inecobank, out _).Should().BeFalse();
        IntegrationCatalogue.All.Should().NotContainKey(IntegrationSlugs.Inecobank);
    }

    /// <summary>The connection test knows the hosted gateway as well as every built-in.</summary>
    [Fact]
    public void ConnectionTestKnowsTheHostedGatewayAndEveryBuiltIn()
    {
        IntegrationCatalogue.TryGetTestable(IntegrationSlugs.Inecobank, out var gateway).Should().BeTrue();
        gateway.RequiredFields.Should().Equal("gateway_url", "username", "password");

        foreach (var slug in IntegrationCatalogue.All.Keys)
        {
            IntegrationCatalogue.TryGetTestable(slug, out _).Should().BeTrue();
        }
    }

    /// <summary>An unknown slug is found by neither lookup.</summary>
    [Fact]
    public void UnknownSlugIsNotFound()
    {
        IntegrationCatalogue.TryGet("no-such-integration", out _).Should().BeFalse();
        IntegrationCatalogue.TryGetTestable("no-such-integration", out _).Should().BeFalse();
    }

    /// <summary>The form and the completeness check have always differed for the card gateway; the catalogue keeps both.</summary>
    [Fact]
    public void StripeKeepsItsFormAndCompletenessRulesApart()
    {
        IntegrationCatalogue.TryGet(IntegrationSlugs.Stripe, out var stripe).Should().BeTrue();

        stripe.Fields.Single(f => f.Key == "publishable_key").Required.Should().BeTrue();
        stripe.RequiredFields.Should().Equal("secret_key");
    }
}
