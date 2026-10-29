using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Admin.Queries.GetSettingValues;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Admin;

/// <summary><see cref="GetSettingValuesQuery"/> through the real <see cref="IMessageBus"/>.</summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetSettingValuesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Seeded keys come back with their values; a key with no row is absent.</summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task KnownKeys_ReturnTheirValues_UnknownKeyIsAbsentAsync()
    {
        var unknown = $"no.such.key.{Guid.NewGuid():N}";

        var result = await InvokeAsync([BillingSettingKeys.InvoiceDaysBefore, BillingSettingKeys.AutoApplyCredit, unknown]);

        result.IsSuccess.Should().BeTrue();
        var keys = result.Value!.Select(s => s.Key).ToList();
        keys.Should().BeEquivalentTo([BillingSettingKeys.AutoApplyCredit, BillingSettingKeys.InvoiceDaysBefore]);
        result.Value!.Should().OnlyContain(s => s.Value != null);
    }

    /// <summary>A duplicate key is answered once.</summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task DuplicateKey_IsAnsweredOnceAsync()
    {
        var result = await InvokeAsync([BillingSettingKeys.ReminderDays, BillingSettingKeys.ReminderDays]);

        result.Value!.Should().ContainSingle(s => s.Key == BillingSettingKeys.ReminderDays);
    }

    /// <summary>Dispatches the query in a fresh scope.</summary>
    /// <param name="keys">The keys to read.</param>
    /// <returns>The query result.</returns>
    private async Task<Result<IReadOnlyList<SettingDto>>> InvokeAsync(IReadOnlyList<string> keys)
    {
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        return await bus.InvokeAsync<Result<IReadOnlyList<SettingDto>>>(new GetSettingValuesQuery(keys), CancellationToken.None);
    }
}
