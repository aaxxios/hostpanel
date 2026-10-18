using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Xunit;
namespace Innovayse.Hostpanel.Domain.Tests.Billing;

/// <summary>Unit tests for the <see cref="Quote"/> aggregate's stage invariants.</summary>
public sealed class QuoteTests
{
    /// <summary>
    /// AddItem on a quote whose stage was moved outside the editable set throws
    /// <see cref="DomainRuleException"/> with <c>QUOTE_NOT_EDITABLE</c>.
    /// </summary>
    [Fact]
    public void AddItem_WhenStageIsDead_ThrowsWithQuoteNotEditable()
    {
        var quote = Quote.Create(clientId: 1, subject: "Test", expiryDate: DateTimeOffset.UtcNow.AddDays(7));
        quote.UpdateDetails(quote.Subject, QuoteStage.Dead, quote.ExpiryDate, quote.Notes);

        var act = () => quote.AddItem("SSL", 5m, 1);

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.QuoteNotEditable);
    }

    /// <summary>Accept on a Dead quote throws <see cref="DomainRuleException"/> with <c>QUOTE_NOT_ACCEPTABLE</c>.</summary>
    [Fact]
    public void Accept_WhenStageIsDead_ThrowsWithQuoteNotAcceptable()
    {
        var quote = Quote.Create(clientId: 1, subject: "Test", expiryDate: DateTimeOffset.UtcNow.AddDays(7));
        quote.UpdateDetails(quote.Subject, QuoteStage.Dead, quote.ExpiryDate, quote.Notes);

        var act = quote.Accept;

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.QuoteNotAcceptable);
    }

    /// <summary>
    /// RemoveItem with an id that names no line on the quote throws <see cref="DomainRuleException"/>
    /// with <c>QUOTE_ITEM_NOT_FOUND</c>.
    /// </summary>
    [Fact]
    public void RemoveItem_WhenIdUnknown_ThrowsWithQuoteItemNotFound()
    {
        var quote = Quote.Create(clientId: 1, subject: "Test", expiryDate: DateTimeOffset.UtcNow.AddDays(7));

        var act = () => quote.RemoveItem(999);

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.QuoteItemNotFound);
    }
}
