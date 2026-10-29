using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Demo;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Demo;

/// <summary>
/// The demo reset also forgets what visitors did to the sample payment methods, so the list comes back as seeded.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DemoCardStateResetTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A changed default and a removed method are back to the seeded list after a reset.</summary>
    [Fact]
    public async Task Reset_RestoresTheSampleMethodsAsync()
    {
        var host = DemoTestHost.Enabled(factory);
        using var scope = host.Services.CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<ICardPaymentGateway>();
        var methods = (await gateway.ListPaymentMethodsAsync(new CardCustomerInstruction("cust-reset"), CancellationToken.None)).Methods;
        await gateway.SetDefaultPaymentMethodAsync(
            new CardDefaultMethodInstruction("cust-reset", methods.Single(m => m.Brand == "mastercard").Id), CancellationToken.None);
        await gateway.DetachPaymentMethodAsync(
            new CardDetachInstruction(methods.Single(m => m.Brand == "Demo Bank").Id), CancellationToken.None);
        var changed = (await gateway.ListPaymentMethodsAsync(new CardCustomerInstruction("cust-reset"), CancellationToken.None)).Methods;

        await scope.ServiceProvider.GetRequiredService<IDemoDataManager>().ResetAsync(CancellationToken.None);
        var restored = (await gateway.ListPaymentMethodsAsync(new CardCustomerInstruction("cust-reset"), CancellationToken.None)).Methods;

        changed.Should().HaveCount(2);
        changed.Single(m => m.IsDefault).Brand.Should().Be("mastercard");
        restored.Should().HaveCount(3);
        restored.Single(m => m.IsDefault).Brand.Should().Be("visa");
    }
}
