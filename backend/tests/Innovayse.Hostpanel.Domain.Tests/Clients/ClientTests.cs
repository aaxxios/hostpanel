using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Domain.Clients.Events;
using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Domain.Tests.Clients;
/// <summary>Unit tests for the <see cref="Client"/> aggregate.</summary>
public class ClientTests
{
    /// <summary>Create should produce Active client with a domain event.</summary>
    [Fact]
    public void Create_ShouldReturnActiveClient_WithDomainEvent()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");

        Assert.Equal("user-1", client.UserId);
        Assert.Equal("John", client.FirstName);
        Assert.Equal(ClientStatus.Active, client.Status);
        Assert.Single(client.DomainEvents);
        Assert.IsType<ClientCreatedEvent>(client.DomainEvents.Single());
    }

    /// <summary>Update should change profile fields.</summary>
    [Fact]
    public void Update_ShouldChangeProfileFields()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.Update("Jane", "Smith", "Acme Corp", "+1-555-0100");

        Assert.Equal("Jane", client.FirstName);
        Assert.Equal("Smith", client.LastName);
        Assert.Equal("Acme Corp", client.CompanyName);
        Assert.Equal("+1-555-0100", client.Phone);
    }

    /// <summary>Suspend should change status to Suspended.</summary>
    [Fact]
    public void Suspend_ShouldSetStatusToSuspended()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.Suspend();

        Assert.Equal(ClientStatus.Suspended, client.Status);
    }

    /// <summary>
    /// Suspending an already suspended client throws <see cref="DomainRuleException"/> with
    /// <c>CLIENT_SUSPEND_NOT_ALLOWED</c>.
    /// </summary>
    [Fact]
    public void Suspend_WhenAlreadySuspended_ThrowsWithClientSuspendNotAllowed()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.Suspend();

        var ex = Assert.Throws<DomainRuleException>(() => client.Suspend());
        Assert.Equal(ClientsErrors.ClientSuspendNotAllowed, ex.Code);
    }

    /// <summary>Activate should restore a suspended client to Active.</summary>
    [Fact]
    public void Activate_WhenSuspended_ShouldSetStatusToActive()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.Suspend();
        client.Activate();

        Assert.Equal(ClientStatus.Active, client.Status);
    }

    /// <summary>Activating an already active account should not throw (idempotent).</summary>
    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotThrow()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.Activate(); // already Active — should be a no-op
        Assert.Equal(ClientStatus.Active, client.Status);
    }

    /// <summary>AddContact should append a contact to the collection.</summary>
    [Fact]
    public void AddContact_ShouldAppendToContacts()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.AddContact(
            "Jane", "Doe", null, "jane@example.com", null, ContactType.Billing,
            null, null, null, null, null, null,
            true, true, true, true, true, true);

        Assert.Single(client.Contacts);
        Assert.Equal("Jane", client.Contacts[0].FirstName);
        Assert.Equal("Doe", client.Contacts[0].LastName);
    }

    /// <summary>RemoveContact should remove the specified contact.</summary>
    [Fact]
    public void RemoveContact_ShouldRemoveFromContacts()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.AddContact(
            "Jane", "Doe", null, "jane@example.com", null, ContactType.Billing,
            null, null, null, null, null, null,
            true, true, true, true, true, true);

        // Contact Id will be 0 since EF Core hasn't assigned a real Id.
        var contactId = client.Contacts[0].Id;
        client.RemoveContact(contactId);

        Assert.Empty(client.Contacts);
    }

    /// <summary>
    /// RemoveContact with an unknown id throws <see cref="DomainRuleException"/> with
    /// <c>CLIENT_CONTACT_NOT_FOUND</c>.
    /// </summary>
    [Fact]
    public void RemoveContact_WhenNotFound_ThrowsWithClientContactNotFound()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        var ex = Assert.Throws<DomainRuleException>(() => client.RemoveContact(999));
        Assert.Equal(ClientsErrors.ClientContactNotFound, ex.Code);
    }

    /// <summary>UpdateContact should update the specified contact's fields.</summary>
    [Fact]
    public void UpdateContact_ShouldUpdateFields()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.AddContact(
            "Jane", "Doe", null, "jane@example.com", null, ContactType.Billing,
            null, null, null, null, null, null,
            true, true, true, true, true, true);

        var contactId = client.Contacts[0].Id;
        client.UpdateContact(
            contactId, "Janet", "Smith", "Acme", "janet@example.com", "+1-555-0100", ContactType.Technical,
            "123 Main St", null, "Springfield", null, "00100", "US",
            false, true, false, true, false, true);

        var contact = client.Contacts[0];
        Assert.Equal("Janet", contact.FirstName);
        Assert.Equal("Smith", contact.LastName);
        Assert.Equal("Acme", contact.CompanyName);
        Assert.Equal("janet@example.com", contact.Email);
        Assert.Equal(ContactType.Technical, contact.Type);
        Assert.Equal("US", contact.Country);
        Assert.False(contact.NotifyGeneral);
    }

    /// <summary>
    /// AddUser with the account owner's own id throws <see cref="DomainRuleException"/> with
    /// <c>CLIENT_USER_IS_OWNER</c>.
    /// </summary>
    [Fact]
    public void AddUser_WhenUserIsOwner_ThrowsWithClientUserIsOwner()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");

        var ex = Assert.Throws<DomainRuleException>(() => client.AddUser("user-1", ClientPermission.None));
        Assert.Equal(ClientsErrors.ClientUserIsOwner, ex.Code);
    }

    /// <summary>
    /// AddUser with a user already linked throws <see cref="DomainRuleException"/> with
    /// <c>CLIENT_USER_LINK_DUPLICATE</c>.
    /// </summary>
    [Fact]
    public void AddUser_WhenAlreadyLinked_ThrowsWithClientUserLinkDuplicate()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.AddUser("user-2", ClientPermission.None);

        var ex = Assert.Throws<DomainRuleException>(() => client.AddUser("user-2", ClientPermission.None));
        Assert.Equal(ClientsErrors.ClientUserLinkDuplicate, ex.Code);
    }

    /// <summary>
    /// RemoveUser with an id that names no linked user throws <see cref="DomainRuleException"/> with
    /// <c>CLIENT_USER_NOT_LINKED</c>.
    /// </summary>
    [Fact]
    public void RemoveUser_WhenNotLinked_ThrowsWithClientUserNotLinked()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");

        var ex = Assert.Throws<DomainRuleException>(() => client.RemoveUser("user-2"));
        Assert.Equal(ClientsErrors.ClientUserNotLinked, ex.Code);
    }

    /// <summary>
    /// TransferOwnership to the current owner throws <see cref="DomainRuleException"/> with
    /// <c>CLIENT_TRANSFER_SAME_OWNER</c>.
    /// </summary>
    [Fact]
    public void TransferOwnership_ToCurrentOwner_ThrowsWithClientTransferSameOwner()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");

        var ex = Assert.Throws<DomainRuleException>(() => client.TransferOwnership("user-1"));
        Assert.Equal(ClientsErrors.ClientTransferSameOwner, ex.Code);
    }

    /// <summary>
    /// DeductCredit for more than the balance throws <see cref="DomainRuleException"/> with
    /// <c>CLIENT_CREDIT_INSUFFICIENT</c>.
    /// </summary>
    [Fact]
    public void DeductCredit_WhenExceedingBalance_ThrowsWithClientCreditInsufficient()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.AddCredit(10m);

        var ex = Assert.Throws<DomainRuleException>(() => client.DeductCredit(20m));
        Assert.Equal(ClientsErrors.ClientCreditInsufficient, ex.Code);
    }

    /// <summary>Closing an already-closed account throws <see cref="DomainRuleException"/> with <c>CLIENT_CLOSE_NOT_ALLOWED</c>.</summary>
    [Fact]
    public void Close_WhenAlreadyClosed_ThrowsWithClientCloseNotAllowed()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.Close();

        var ex = Assert.Throws<DomainRuleException>(() => client.Close());
        Assert.Equal(ClientsErrors.ClientCloseNotAllowed, ex.Code);
    }

    /// <summary>Activating a closed account throws <see cref="DomainRuleException"/> with <c>CLIENT_ACTIVATE_NOT_ALLOWED</c>.</summary>
    [Fact]
    public void Activate_WhenClosed_ThrowsWithClientActivateNotAllowed()
    {
        var client = Client.Create("user-1", "John", "Doe", "john@example.com");
        client.Close();

        var ex = Assert.Throws<DomainRuleException>(() => client.Activate());
        Assert.Equal(ClientsErrors.ClientActivateNotAllowed, ex.Code);
    }

    /// <summary>A code is stored upper case whatever the caller typed, so "amd" and "AMD" are one currency.</summary>
    [Fact]
    public void SetCurrency_UpperCasesTheCode()
    {
        var client = Client.Create("user-1", "Jane", "Doe", "jane@example.com");

        client.SetCurrency("amd");

        Assert.Equal("AMD", client.Currency);
    }

    /// <summary>Anything but a three-letter code is refused: an invoice cannot be raised in "US$".</summary>
    /// <param name="code">A currency code that is not three letters.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void SetCurrency_RejectsACodeThatIsNotThreeLetters(string code)
    {
        var client = Client.Create("user-1", "Jane", "Doe", "jane@example.com");

        Assert.Throws<ArgumentException>(() => client.SetCurrency(code));
        Assert.Null(client.Currency);
    }

    /// <summary>The other preferences no longer carry the currency, so writing them leaves it alone.</summary>
    [Fact]
    public void UpdatePreferences_DoesNotTouchTheCurrency()
    {
        var client = Client.Create("user-1", "Jane", "Doe", "jane@example.com");
        client.SetCurrency("USD");

        client.UpdatePreferences("bank_transfer", "billing@example.com", "note");

        Assert.Equal("USD", client.Currency);
        Assert.Equal("bank_transfer", client.PaymentMethod);
    }
}
