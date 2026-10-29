using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Clients;

/// <summary>Unit tests for <see cref="Invitation"/>.</summary>
public sealed class InvitationTests
{
    /// <summary>The moment every invitation in these tests is issued.</summary>
    private static readonly DateTimeOffset IssuedAt = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

    /// <summary>Create sets the expiry seven days after the moment the caller passes.</summary>
    [Fact]
    public void Create_ExpiresSevenDaysAfterIssue()
    {
        var invitation = NewInvitation();

        invitation.ExpiresAt.Should().Be(IssuedAt.AddDays(7));
        invitation.IsExpiredAt(IssuedAt.AddDays(7).AddTicks(-1)).Should().BeFalse();
        invitation.IsExpiredAt(IssuedAt.AddDays(7)).Should().BeTrue();
    }

    /// <summary>Accept stamps the moment the caller passes.</summary>
    [Fact]
    public void Accept_StampsTheAcceptanceTime()
    {
        var invitation = NewInvitation();
        var acceptedAt = IssuedAt.AddDays(2);

        invitation.Accept(acceptedAt);

        invitation.AcceptedAt.Should().Be(acceptedAt);
        invitation.IsAccepted.Should().BeTrue();
    }

    /// <summary>Accept refuses an invitation that has expired at the moment of acceptance.</summary>
    [Fact]
    public void Accept_AfterExpiry_Throws()
    {
        var invitation = NewInvitation();

        var act = () => invitation.Accept(IssuedAt.AddDays(8));

        act.Should().Throw<InvalidOperationException>();
        invitation.AcceptedAt.Should().BeNull();
    }

    /// <summary>Builds an invitation issued at <see cref="IssuedAt"/>.</summary>
    /// <returns>The invitation.</returns>
    private static Invitation NewInvitation() =>
        Invitation.Create(1, "invitee@example.test", "Ina", "Vitee", ClientPermission.ViewProductsServices, IssuedAt);
}
