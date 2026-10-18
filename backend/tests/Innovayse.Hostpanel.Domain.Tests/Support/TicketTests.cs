using FluentAssertions;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Xunit;
namespace Innovayse.Hostpanel.Domain.Tests.Support;

/// <summary>Unit tests for the <see cref="Ticket"/> aggregate's status invariants.</summary>
public sealed class TicketTests
{
    /// <summary>AddReply on a closed ticket throws <see cref="DomainRuleException"/> with <c>TICKET_REPLY_NOT_ALLOWED</c>.</summary>
    [Fact]
    public void AddReply_WhenClosed_ThrowsWithTicketReplyNotAllowed()
    {
        var ticket = Ticket.Create(clientId: 1, subject: "Help", message: "Hi", departmentId: 1, priority: TicketPriority.Medium);
        ticket.Close();

        var act = () => ticket.AddReply("Still there?", "Agent", isStaffReply: true);

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(SupportErrors.TicketReplyNotAllowed);
    }

    /// <summary>Close on an already-closed ticket throws <see cref="DomainRuleException"/> with <c>TICKET_CLOSE_NOT_ALLOWED</c>.</summary>
    [Fact]
    public void Close_WhenAlreadyClosed_ThrowsWithTicketCloseNotAllowed()
    {
        var ticket = Ticket.Create(clientId: 1, subject: "Help", message: "Hi", departmentId: 1, priority: TicketPriority.Medium);
        ticket.Close();

        var act = ticket.Close;

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(SupportErrors.TicketCloseNotAllowed);
    }
}
