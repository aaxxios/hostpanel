using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Orders;

/// <summary>Unit tests for the <see cref="Order"/> aggregate's status invariant.</summary>
public sealed class OrderTests
{
    /// <summary>Accept on a non-Pending order throws <see cref="DomainRuleException"/> with <c>ORDER_NOT_PENDING</c>.</summary>
    [Fact]
    public void Accept_WhenNotPending_ThrowsWithOrderNotPending()
    {
        var order = Order.Create("ORD-0001", clientId: 1, paymentMethod: "manual", ipAddress: null);
        order.Accept();

        var act = order.Accept;

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(OrdersErrors.OrderNotPending);
    }

    /// <summary>Cancel on a non-Pending order throws <see cref="DomainRuleException"/> with <c>ORDER_NOT_PENDING</c>.</summary>
    [Fact]
    public void Cancel_WhenNotPending_ThrowsWithOrderNotPending()
    {
        var order = Order.Create("ORD-0002", clientId: 1, paymentMethod: "manual", ipAddress: null);
        order.Accept();

        var act = order.Cancel;

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(OrdersErrors.OrderNotPending);
    }

    /// <summary>MarkFraud on a non-Pending order throws <see cref="DomainRuleException"/> with <c>ORDER_NOT_PENDING</c>.</summary>
    [Fact]
    public void MarkFraud_WhenNotPending_ThrowsWithOrderNotPending()
    {
        var order = Order.Create("ORD-0003", clientId: 1, paymentMethod: "manual", ipAddress: null);
        order.Accept();

        var act = order.MarkFraud;

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(OrdersErrors.OrderNotPending);
    }

    /// <summary>Every new order carries a token, so no order is ever payable by id alone.</summary>
    [Fact]
    public void Create_AlwaysIssuesAPaymentToken()
    {
        var order = Order.Create("ORD-0001", 1, PaymentMethodNames.Card, null);

        Assert.False(string.IsNullOrWhiteSpace(order.PaymentToken));
    }

    /// <summary>
    /// Two orders never share a token. A shared or derived token would make one order's token a
    /// key to every other, which is the whole failure this field exists to prevent.
    /// </summary>
    [Fact]
    public void Create_IssuesADifferentTokenPerOrder()
    {
        var first = Order.Create("ORD-0001", 1, PaymentMethodNames.Card, null);
        var second = Order.Create("ORD-0002", 1, PaymentMethodNames.Card, null);

        Assert.NotEqual(first.PaymentToken, second.PaymentToken);
    }

    /// <summary>
    /// The token is long enough that guessing is not a strategy. 32 random bytes encode to 43
    /// base64url characters; a token materially shorter than that would be a silent weakening.
    /// </summary>
    [Fact]
    public void Create_IssuesATokenOfAtLeastFortyThreeCharacters()
    {
        var order = Order.Create("ORD-0001", 1, PaymentMethodNames.Card, null);

        Assert.True(
            order.PaymentToken.Length >= 43,
            $"Payment token was {order.PaymentToken.Length} characters; expected at least 43.");
    }

    /// <summary>The order's own token is accepted.</summary>
    [Fact]
    public void MatchesPaymentToken_WithTheIssuedToken_ReturnsTrue()
    {
        var order = Order.Create("ORD-0001", 1, PaymentMethodNames.Card, null);

        Assert.True(order.MatchesPaymentToken(order.PaymentToken));
    }

    /// <summary>Another order's token is refused.</summary>
    [Fact]
    public void MatchesPaymentToken_WithAnotherOrdersToken_ReturnsFalse()
    {
        var order = Order.Create("ORD-0001", 1, PaymentMethodNames.Card, null);
        var other = Order.Create("ORD-0002", 1, PaymentMethodNames.Card, null);

        Assert.False(order.MatchesPaymentToken(other.PaymentToken));
    }

    /// <summary>
    /// A caller that sends nothing is refused. This is the case that matters most: the endpoints
    /// took no token at all before, so an old client — or an attacker walking the id range —
    /// arrives with none, and must not be treated as authorised.
    /// </summary>
    /// <param name="token">The absent or blank token a caller might send.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void MatchesPaymentToken_WithNoToken_ReturnsFalse(string? token)
    {
        var order = Order.Create("ORD-0001", 1, PaymentMethodNames.Card, null);

        Assert.False(order.MatchesPaymentToken(token));
    }

    /// <summary>A token that is merely a prefix of the real one is refused.</summary>
    [Fact]
    public void MatchesPaymentToken_WithAPrefixOfTheToken_ReturnsFalse()
    {
        var order = Order.Create("ORD-0001", 1, PaymentMethodNames.Card, null);

        Assert.False(order.MatchesPaymentToken(order.PaymentToken[..^1]));
    }

    /// <summary>Backdating moves the placement date.</summary>
    [Fact]
    public void Backdate_MovesCreatedAt()
    {
        var order = Order.Create("DEMO-1", 1, "card", null);
        var earlier = DateTimeOffset.UtcNow.AddMonths(-5);

        order.Backdate(earlier);

        order.CreatedAt.Should().Be(earlier);
    }
}
