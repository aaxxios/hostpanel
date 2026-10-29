using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Domain.Tests.Billing;

/// <summary>Unit tests for <see cref="Transaction"/>: currency, the money invariant and the correction methods.</summary>
public sealed class TransactionTests
{
    /// <summary>The fixed moment the invoice in these tests is issued.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

    /// <summary>The currency given at creation is what the transaction reports, upper-cased.</summary>
    [Fact]
    public void Create_StampsTheCurrency()
    {
        var transaction = Transaction.Create(
            1, Now, "Top-up", "tx-1", null, "manual", 10m, 0m, 0m, false, "eur");

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
            1, Now, "Top-up", "tx-1", null, "manual", 10m, 0m, 0m, false, currency);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>A payment row takes the currency of the invoice it settles.</summary>
    [Fact]
    public void CreateInvoicePayment_TakesTheInvoiceCurrency()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: Now.AddDays(7), currency: "GBP", issuedAt: Now);

        var transaction = Transaction.CreateInvoicePayment(
            invoice, Now, "manual", "tx-2", 25m, 0m);

        transaction.Currency.Should().Be("GBP");
    }

    /// <summary>A transaction that moves no money in either direction is refused with the domain code.</summary>
    [Fact]
    public void Create_RefusesZeroAmounts()
    {
        var act = () => Transaction.Create(
            1, Now, "Nothing", "tx-0", null, "manual", 0m, 0m, 0m, false, "USD");

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.TransactionAmountRequired);
    }

    /// <summary>Correcting the details changes date, description, reference and method, and leaves the amounts alone.</summary>
    [Fact]
    public void CorrectDetails_ChangesTheRecordedDetailsOnly()
    {
        var transaction = NewTransaction();
        var date = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

        transaction.CorrectDetails(date, "Corrected", "tx-9", "bank");

        transaction.Date.Should().Be(date);
        transaction.Description.Should().Be("Corrected");
        transaction.TransactionId.Should().Be("tx-9");
        transaction.PaymentMethod.Should().Be("bank");
        transaction.AmountIn.Should().Be(10m);
    }

    /// <summary>Linking sets the invoice, and null detaches it again.</summary>
    [Fact]
    public void LinkToInvoice_SetsAndClearsTheInvoice()
    {
        var transaction = NewTransaction();

        transaction.LinkToInvoice(42);
        transaction.InvoiceId.Should().Be(42);

        transaction.LinkToInvoice(null);
        transaction.InvoiceId.Should().BeNull();
    }

    /// <summary>Correcting the amounts replaces all three together.</summary>
    [Fact]
    public void CorrectAmounts_ReplacesTheAmounts()
    {
        var transaction = NewTransaction();

        transaction.CorrectAmounts(0m, 7m, 1m);

        transaction.AmountIn.Should().Be(0m);
        transaction.AmountOut.Should().Be(7m);
        transaction.Fees.Should().Be(1m);
    }

    /// <summary>A correction that would leave no money moved is refused and the amounts stay as they were.</summary>
    [Fact]
    public void CorrectAmounts_RefusesZeroAmounts()
    {
        var transaction = NewTransaction();

        var act = () => transaction.CorrectAmounts(0m, 0m, 0m);

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.TransactionAmountRequired);
        transaction.AmountIn.Should().Be(10m);
    }

    /// <summary>Builds a 10 USD incoming transaction.</summary>
    /// <returns>The new transaction.</returns>
    private static Transaction NewTransaction()
    {
        return Transaction.Create(1, Now, "Top-up", "tx-1", null, "manual", 10m, 0m, 0m, false, "USD");
    }
}
