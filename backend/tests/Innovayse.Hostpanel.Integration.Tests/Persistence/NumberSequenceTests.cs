using Innovayse.Hostpanel.Application.Common.Enums;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Persistence;

/// <summary>
/// Pins <see cref="INumberSequence"/>: callers asking at the same moment, each on its own
/// connection, are all handed a different number.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class NumberSequenceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Many concurrent callers receive distinct, increasing-per-caller numbers.</summary>
    [Fact]
    public async Task ConcurrentCallersReceiveDistinctNumbers()
    {
        // Arrange
        const int Callers = 40;

        // Act
        var numbers = await Task.WhenAll(Enumerable.Range(0, Callers).Select(async _ =>
        {
            using var scope = factory.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<INumberSequence>()
                .NextAsync(NumberKind.Order, CancellationToken.None);
        }));

        // Assert
        numbers.Should().OnlyHaveUniqueItems();
        numbers.Should().OnlyContain(n => n > 0);
    }

    /// <summary>A later call never receives a number an earlier one already did.</summary>
    [Fact]
    public async Task SuccessiveCallsAdvance()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var sequence = scope.ServiceProvider.GetRequiredService<INumberSequence>();

        // Act
        var first = await sequence.NextAsync(NumberKind.Order, CancellationToken.None);
        var second = await sequence.NextAsync(NumberKind.Order, CancellationToken.None);

        // Assert
        second.Should().BeGreaterThan(first);
    }
}
