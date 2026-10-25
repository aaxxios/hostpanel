using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Billing;

/// <summary>Unit tests for the currency a <see cref="Transaction"/> carries.</summary>
public sealed class TransactionTests
{
    /// <summary>The currency given at creation is what the transaction reports, upper-cased.</summary>
    [Fact]
    public void Create_StampsTheCurrency()
    {
        var transaction = Transaction.Create(
            1, DateTimeOffset.UtcNow, "Top-up", "tx-1", null, "manual", 10m, 0m, 0m, false, "eur");

        transaction.Currency.Should().Be("EUR");
    }

    /// <summary>A transaction without a valid currency is refused, so no ledger row is ever unit-less.</summary>
    /// <param name="currency">A missing or malformed code.</param>
    [Theory]
    [InlineData("")]
    [InlineData("US")]
    public void Create_RefusesAMalformedCurrency(string currency)
    {
        var act = () => Transaction.Create(
            1, DateTimeOffset.UtcNow, "Top-up", "tx-1", null, "manual", 10m, 0m, 0m, false, currency);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>A payment row takes the currency of the invoice it settles.</summary>
    [Fact]
    public void CreateInvoicePayment_TakesTheInvoiceCurrency()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "GBP");

        var transaction = Transaction.CreateInvoicePayment(
            invoice, DateTimeOffset.UtcNow, "manual", "tx-2", 25m, 0m);

        transaction.Currency.Should().Be("GBP");
    }
}
