using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Products;

namespace Innovayse.Hostpanel.Domain.Tests.Orders;
/// <summary>Unit tests for <see cref="OrderItem"/> domain fields.</summary>
public sealed class OrderItemTests
{
    /// <summary>AddItem with domain action stores all domain fields on the order item.</summary>
    [Fact]
    public void AddItem_WithDomainAction_StoresDomainFields()
    {
        var order = Order.Create("ORD-0001", 1, PaymentMethodNames.Card, null);

        order.AddItem(
            productId: 99,
            productName: "Domain Registration",
            billingCycle: BillingCycleNames.Annually,
            firstPaymentAmount: 12.99m,
            recurringAmount: 12.99m,
            domain: "example.com",
            hostname: null,
            domainAction: DomainActionNames.Register,
            eppCode: null,
            years: 1);

        var item = order.Items[0];
        Assert.Equal(DomainActionNames.Register, item.DomainAction);
        Assert.Null(item.EppCode);
        Assert.Equal(1, item.Years);
    }

    /// <summary>AddItem with transfer action stores EPP code.</summary>
    [Fact]
    public void AddItem_WithTransferAction_StoresEppCode()
    {
        var order = Order.Create("ORD-0002", 1, PaymentMethodNames.Card, null);

        order.AddItem(
            productId: 99,
            productName: "Domain Registration",
            billingCycle: BillingCycleNames.Annually,
            firstPaymentAmount: 12.99m,
            recurringAmount: 12.99m,
            domain: "transfer.com",
            hostname: null,
            domainAction: DomainActionNames.Transfer,
            eppCode: "EPP-ABC-123",
            years: 1);

        var item = order.Items[0];
        Assert.Equal(DomainActionNames.Transfer, item.DomainAction);
        Assert.Equal("EPP-ABC-123", item.EppCode);
    }

    /// <summary>AddItem without domain action leaves domain fields null (hosting item).</summary>
    [Fact]
    public void AddItem_WithoutDomainAction_LeavesFieldsNull()
    {
        var order = Order.Create("ORD-0003", 1, PaymentMethodNames.Card, null);

        order.AddItem(
            productId: 5,
            productName: "Starter Hosting",
            billingCycle: BillingCycleNames.Monthly,
            firstPaymentAmount: 4.99m,
            recurringAmount: 4.99m,
            domain: null,
            hostname: null,
            domainAction: null,
            eppCode: null,
            years: null);

        var item = order.Items[0];
        Assert.Null(item.DomainAction);
        Assert.Null(item.EppCode);
        Assert.Null(item.Years);
    }
}
