using System.Security.Claims;
using FluentAssertions;
using Innovayse.SDK.Plugins;
using Innovayse.SDK.Plugins.Enums;
using Innovayse.SDK.Plugins.Fulfilment;
using Innovayse.SDK.Plugins.Hosting;
using Innovayse.SDK.Plugins.Identity;
using Xunit;

namespace Innovayse.Hostpanel.Architecture.Tests;
/// <summary>
/// Pins the plugin SDK's public surface. A third party compiles against the published package,
/// so a renamed member here is a breaking change for every plugin in the wild; these tests make
/// that a red build rather than a discovery at the consumer's end.
/// </summary>
public sealed class SdkContractTests
{
    /// <summary>The three plugin types P1 adds exist beside the original three and keep their order.</summary>
    [Fact]
    public void PluginType_CarriesTheSixKinds()
    {
        Enum.GetNames<PluginType>().Should().Equal(
            "Provisioning", "Payment", "Registrar", "Identity", "Fulfilment", "Hosted");
    }

    /// <summary>
    /// An identity plugin answers with the external account the principal acts for and two capabilities.
    /// </summary>
    [Fact]
    public void IClientResolver_ResolvesAPrincipalToClientAccess()
    {
        var method = typeof(IClientResolver).GetMethod(nameof(IClientResolver.ResolveAsync));

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<ClientAccess?>));
        method.GetParameters().Select(p => p.ParameterType)
            .Should().Equal(typeof(ClaimsPrincipal), typeof(CancellationToken));
        typeof(ClientAccess).GetProperties().Select(p => p.Name)
            .Should().BeEquivalentTo("Account", "CanPurchase", "CanManagePaymentMethods");
    }

    /// <summary>A fulfilment plugin has exactly the four hooks the host calls, in the host's vocabulary.</summary>
    [Fact]
    public void IFulfilmentPlugin_HasTheFourHooks()
    {
        typeof(IFulfilmentPlugin).GetMethods().Select(m => m.Name).Should().BeEquivalentTo(
            "OnInvoicePaidAsync", "AdviseRefundAsync", "OnInvoiceRefundedAsync", "OnSubscriptionLapsedAsync");
        typeof(IFulfilmentPlugin).GetMethod("AdviseRefundAsync")!.ReturnType
            .Should().Be(typeof(Task<RefundAdvice>));
    }

    /// <summary>Every fulfilment context carries the invoice, the client, the external account and the lines.</summary>
    /// <param name="context">The fulfilment context type under inspection.</param>
    [Theory]
    [InlineData(typeof(InvoicePaidContext))]
    [InlineData(typeof(RefundAdviceContext))]
    [InlineData(typeof(InvoiceRefundedContext))]
    [InlineData(typeof(SubscriptionLapsedContext))]
    public void FulfilmentContexts_ShareTheCommonShape(Type context)
    {
        var names = context.GetProperties().Select(p => p.Name).ToList();

        names.Should().Contain(["InvoiceId", "ClientId", "Account", "Lines"]);
        context.GetProperty("Lines")!.PropertyType.Should().Be(typeof(IReadOnlyList<FulfilmentLine>));
    }

    /// <summary>A hosted plugin is started and stopped by the host and nothing else.</summary>
    [Fact]
    public void IHostedPlugin_HasStartAndStop()
    {
        typeof(IHostedPlugin).GetMethods().Select(m => m.Name)
            .Should().BeEquivalentTo("StartAsync", "StopAsync");
    }
}
