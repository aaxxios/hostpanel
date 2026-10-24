using FluentAssertions;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Xunit;
namespace Innovayse.Hostpanel.Domain.Tests.Services;

/// <summary>Unit tests for the <see cref="CancellationRequest"/> entity.</summary>
public sealed class CancellationRequestTests
{
    /// <summary>Backdate moves the request to the day it was made and keeps it open.</summary>
    [Fact]
    public void Backdate_MovesTheCreationDateOnly()
    {
        var request = CancellationRequest.Create(serviceId: 1, CancellationType.EndOfBillingPeriod, "Budget cut");
        var made = new DateTimeOffset(2026, 9, 2, 9, 0, 0, TimeSpan.Zero);

        request.Backdate(made);

        request.CreatedAt.Should().Be(made);
        request.Status.Should().Be(CancellationStatus.Open);
    }
}
