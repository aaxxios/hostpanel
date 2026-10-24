using FluentAssertions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Innovayse.Hostpanel.SharedKernel.Domain;
namespace Innovayse.Hostpanel.Domain.Tests.Domains;
/// <summary>Unit tests for the <see cref="RegisteredDomain"/> aggregate lifecycle.</summary>
public sealed class RegisteredDomainTests
{
    // ── Register / Activate ──────────────────────────────────────────────────

    /// <summary>Register, should create domain with pending registration status.</summary>
    [Fact]
    public void Register_ShouldCreateDomainWithPendingRegistrationStatus()
    {
        var domain = RegisteredDomain.Register(1, "example.com", DateTimeOffset.UtcNow.AddYears(1), true, true);

        domain.Status.Should().Be(DomainStatus.PendingRegistration);
        domain.ClientId.Should().Be(1);
        domain.Name.Should().Be("example.com");
        domain.Tld.Should().Be("com");
        domain.AutoRenew.Should().BeTrue();
        domain.WhoisPrivacy.Should().BeTrue();
    }

    /// <summary>Activate, after register, should transition to active and raise domain registered event.</summary>
    [Fact]
    public void Activate_AfterRegister_ShouldTransitionToActiveAndRaiseDomainRegisteredEvent()
    {
        var domain = RegisteredDomain.Register(1, "example.com", DateTimeOffset.UtcNow.AddYears(1), true, false);

        domain.Activate("REG-001");

        domain.Status.Should().Be(DomainStatus.Active);
        domain.RegistrarRef.Should().Be("REG-001");
        domain.DomainEvents.Should().ContainSingle(e => e is DomainRegisteredEvent);
        var evt = (DomainRegisteredEvent)domain.DomainEvents.Single();
        evt.ClientId.Should().Be(1);
        evt.Name.Should().Be("example.com");
    }

    /// <summary>Activate, when not pending registration, should throw invalid operation exception.</summary>
    [Fact]
    public void Activate_WhenNotPendingRegistration_ShouldThrowInvalidOperationException()
    {
        var domain = RegisteredDomain.Register(1, "example.com", DateTimeOffset.UtcNow.AddYears(1), false, false);
        domain.Activate("REG-001");

        var act = () => domain.Activate("REG-002");

        act.Should().Throw<InvalidOperationException>();
    }

    // ── CreateTransfer / ActivateTransfer ────────────────────────────────────

    /// <summary>Create transfer, should create domain with pending transfer status.</summary>
    [Fact]
    public void CreateTransfer_ShouldCreateDomainWithPendingTransferStatus()
    {
        var domain = RegisteredDomain.CreateTransfer(2, "transfer.net");

        domain.Status.Should().Be(DomainStatus.PendingTransfer);
        domain.ClientId.Should().Be(2);
        domain.Name.Should().Be("transfer.net");
        domain.Tld.Should().Be("net");
    }

    /// <summary>Activate transfer, should transition to active and raise domain transferred in event.</summary>
    [Fact]
    public void ActivateTransfer_ShouldTransitionToActiveAndRaiseDomainTransferredInEvent()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddYears(2);
        var domain = RegisteredDomain.CreateTransfer(2, "transfer.net");

        domain.ActivateTransfer("REG-TR-001", expiresAt);

        domain.Status.Should().Be(DomainStatus.Active);
        domain.RegistrarRef.Should().Be("REG-TR-001");
        domain.ExpiresAt.Should().Be(expiresAt);
        domain.DomainEvents.Should().ContainSingle(e => e is DomainTransferredInEvent);
        var evt = (DomainTransferredInEvent)domain.DomainEvents.Single();
        evt.ClientId.Should().Be(2);
        evt.Name.Should().Be("transfer.net");
    }

    // ── MarkExpired ──────────────────────────────────────────────────────────

    /// <summary>Mark expired, when active, should transition to expired and raise domain expired event.</summary>
    [Fact]
    public void MarkExpired_WhenActive_ShouldTransitionToExpiredAndRaiseDomainExpiredEvent()
    {
        var domain = CreateActiveDomain();

        domain.MarkExpired();

        domain.Status.Should().Be(DomainStatus.Expired);
        domain.DomainEvents.OfType<DomainExpiredEvent>().Should().ContainSingle();
    }

    /// <summary>Mark expired, when not active, should throw invalid operation exception.</summary>
    [Fact]
    public void MarkExpired_WhenNotActive_ShouldThrowInvalidOperationException()
    {
        var domain = RegisteredDomain.Register(1, "example.com", DateTimeOffset.UtcNow.AddYears(1), false, false);

        var act = () => domain.MarkExpired();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── MarkRedemption ───────────────────────────────────────────────────────

    /// <summary>Mark redemption, when expired, should transition to redemption.</summary>
    [Fact]
    public void MarkRedemption_WhenExpired_ShouldTransitionToRedemption()
    {
        var domain = CreateActiveDomain();
        domain.MarkExpired();

        domain.MarkRedemption();

        domain.Status.Should().Be(DomainStatus.Redemption);
    }

    /// <summary>Mark redemption, when not expired, should throw invalid operation exception.</summary>
    [Fact]
    public void MarkRedemption_WhenNotExpired_ShouldThrowInvalidOperationException()
    {
        var domain = CreateActiveDomain();

        var act = () => domain.MarkRedemption();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── MarkTransferred ──────────────────────────────────────────────────────

    /// <summary>Mark transferred, when active, should transition to transferred.</summary>
    [Fact]
    public void MarkTransferred_WhenActive_ShouldTransitionToTransferred()
    {
        var domain = CreateActiveDomain();

        domain.MarkTransferred();

        domain.Status.Should().Be(DomainStatus.Transferred);
    }

    // ── Cancel ───────────────────────────────────────────────────────────────

    /// <summary>Cancel, when pending registration, should transition to cancelled.</summary>
    [Fact]
    public void Cancel_WhenPendingRegistration_ShouldTransitionToCancelled()
    {
        var domain = RegisteredDomain.Register(1, "example.com", DateTimeOffset.UtcNow.AddYears(1), false, false);

        domain.Cancel();

        domain.Status.Should().Be(DomainStatus.Cancelled);
    }

    /// <summary>Cancel, when active, throws with domain cancel not allowed.</summary>
    [Fact]
    public void Cancel_WhenActive_ThrowsWithDomainCancelNotAllowed()
    {
        var domain = CreateActiveDomain();

        var act = () => domain.Cancel();

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(DomainsErrors.DomainCancelNotAllowed);
    }

    /// <summary>Cancel, when transferred, throws with domain cancel not allowed.</summary>
    [Fact]
    public void Cancel_WhenTransferred_ThrowsWithDomainCancelNotAllowed()
    {
        var domain = CreateActiveDomain();
        domain.MarkTransferred();

        var act = () => domain.Cancel();

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(DomainsErrors.DomainCancelNotAllowed);
    }

    /// <summary>Cancel, when already cancelled, throws with domain cancel not allowed.</summary>
    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsWithDomainCancelNotAllowed()
    {
        var domain = RegisteredDomain.Register(1, "example.com", DateTimeOffset.UtcNow.AddYears(1), false, false);
        domain.Cancel();

        var act = () => domain.Cancel();

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(DomainsErrors.DomainCancelNotAllowed);
    }

    // ── Renew ────────────────────────────────────────────────────────────────

    /// <summary>Renew, when active, should extend expires at and raise domain renewed event.</summary>
    [Fact]
    public void Renew_WhenActive_ShouldExtendExpiresAtAndRaiseDomainRenewedEvent()
    {
        var domain = CreateActiveDomain();
        var newExpiry = DateTimeOffset.UtcNow.AddYears(2);

        domain.Renew(newExpiry);

        domain.ExpiresAt.Should().Be(newExpiry);
        domain.DomainEvents.OfType<DomainRenewedEvent>().Should().ContainSingle();
    }

    /// <summary>Renew, when cancelled, throws with domain renew not allowed.</summary>
    [Fact]
    public void Renew_WhenCancelled_ThrowsWithDomainRenewNotAllowed()
    {
        var domain = RegisteredDomain.Register(1, "example.com", DateTimeOffset.UtcNow.AddYears(1), false, false);
        domain.Cancel();

        var act = () => domain.Renew(DateTimeOffset.UtcNow.AddYears(2));

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(DomainsErrors.DomainRenewNotAllowed);
    }

    /// <summary>Renew, when transferred, throws with domain renew not allowed.</summary>
    [Fact]
    public void Renew_WhenTransferred_ThrowsWithDomainRenewNotAllowed()
    {
        var domain = CreateActiveDomain();
        domain.MarkTransferred();

        var act = () => domain.Renew(DateTimeOffset.UtcNow.AddYears(2));

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(DomainsErrors.DomainRenewNotAllowed);
    }

    // ── Flag updates ─────────────────────────────────────────────────────────

    /// <summary>Set auto renew, should update flag.</summary>
    [Fact]
    public void SetAutoRenew_ShouldUpdateFlag()
    {
        var domain = CreateActiveDomain();

        domain.SetAutoRenew(false);

        domain.AutoRenew.Should().BeFalse();
    }

    /// <summary>Set whois privacy, should update flag.</summary>
    [Fact]
    public void SetWhoisPrivacy_ShouldUpdateFlag()
    {
        var domain = CreateActiveDomain();

        domain.SetWhoisPrivacy(true);

        domain.WhoisPrivacy.Should().BeTrue();
    }

    /// <summary>Set lock, should update flag.</summary>
    [Fact]
    public void SetLock_ShouldUpdateFlag()
    {
        var domain = CreateActiveDomain();

        domain.SetLock(true);

        domain.IsLocked.Should().BeTrue();
    }

    /// <summary>Set epp code, should update code.</summary>
    [Fact]
    public void SetEppCode_ShouldUpdateCode()
    {
        var domain = CreateActiveDomain();

        domain.SetEppCode("EPP-XYZ");

        domain.EppCode.Should().Be("EPP-XYZ");
    }

    // ── Nameservers ──────────────────────────────────────────────────────────

    /// <summary>Set nameservers, should replace existing nameservers.</summary>
    [Fact]
    public void SetNameservers_ShouldReplaceExistingNameservers()
    {
        var domain = CreateActiveDomain();
        domain.SetNameservers(["ns1.old.com", "ns2.old.com"]);

        domain.SetNameservers(["ns1.new.com", "ns2.new.com", "ns3.new.com"]);

        domain.Nameservers.Should().HaveCount(3);
        domain.Nameservers.Select(n => n.Host).Should().Contain("ns1.new.com");
    }

    // ── DNS records ──────────────────────────────────────────────────────────

    /// <summary>Add dns record, should add record to collection.</summary>
    [Fact]
    public void AddDnsRecord_ShouldAddRecordToCollection()
    {
        var domain = CreateActiveDomain();

        domain.AddDnsRecord(DnsRecordType.A, "www", "1.2.3.4", 3600, null);

        domain.DnsRecords.Should().HaveCount(1);
        domain.DnsRecords[0].Type.Should().Be(DnsRecordType.A);
        domain.DnsRecords[0].Host.Should().Be("www");
        domain.DnsRecords[0].Value.Should().Be("1.2.3.4");
    }

    /// <summary>Remove dns record, should remove record by id.</summary>
    [Fact]
    public void RemoveDnsRecord_ShouldRemoveRecordById()
    {
        var domain = CreateActiveDomain();
        domain.AddDnsRecord(DnsRecordType.TXT, "@", "v=spf1 include:example.com ~all", 300, null);
        var record = domain.DnsRecords[0];

        domain.RemoveDnsRecord(record.Id);

        domain.DnsRecords.Should().BeEmpty();
    }

    /// <summary>Remove dns record, when not found, throws with domain dns record not found.</summary>
    [Fact]
    public void RemoveDnsRecord_WhenNotFound_ThrowsWithDomainDnsRecordNotFound()
    {
        var domain = CreateActiveDomain();

        var act = () => domain.RemoveDnsRecord(999);

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(DomainsErrors.DomainDnsRecordNotFound);
    }

    /// <summary>Update dns record, should update value and ttl.</summary>
    [Fact]
    public void UpdateDnsRecord_ShouldUpdateValueAndTtl()
    {
        var domain = CreateActiveDomain();
        domain.AddDnsRecord(DnsRecordType.A, "mail", "1.2.3.4", 3600, null);
        var record = domain.DnsRecords[0];

        domain.UpdateDnsRecord(record.Id, "5.6.7.8", 7200, null);

        domain.DnsRecords[0].Value.Should().Be("5.6.7.8");
        domain.DnsRecords[0].Ttl.Should().Be(7200);
    }

    /// <summary>Update dns record, when not found, throws with domain dns record not found.</summary>
    [Fact]
    public void UpdateDnsRecord_WhenNotFound_ThrowsWithDomainDnsRecordNotFound()
    {
        var domain = CreateActiveDomain();

        var act = () => domain.UpdateDnsRecord(999, "value", 3600, null);

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(DomainsErrors.DomainDnsRecordNotFound);
    }

    /// <summary>
    /// UpdateForwardingRule with an id that names no rule throws <see cref="DomainRuleException"/>
    /// with <c>DOMAIN_FORWARDING_RULE_NOT_FOUND</c>.
    /// </summary>
    [Fact]
    public void UpdateForwardingRule_WhenNotFound_ThrowsWithDomainForwardingRuleNotFound()
    {
        var domain = CreateActiveDomain();

        var act = () => domain.UpdateForwardingRule(999, "info", "dest@example.com", true);

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(DomainsErrors.DomainForwardingRuleNotFound);
    }

    // ── LinkService ──────────────────────────────────────────────────────────

    /// <summary>Link service, should set linked service id.</summary>
    [Fact]
    public void LinkService_ShouldSetLinkedServiceId()
    {
        var domain = CreateActiveDomain();

        domain.LinkService(42);

        domain.LinkedServiceId.Should().Be(42);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Creates an active domain for use in tests that require an Active status.</summary>
    /// <returns>An active <see cref="RegisteredDomain"/> aggregate.</returns>
    private static RegisteredDomain CreateActiveDomain()
    {
        var domain = RegisteredDomain.Register(1, "example.com", DateTimeOffset.UtcNow.AddYears(1), true, false);
        domain.Activate("REG-TEST");
        domain.ClearDomainEvents();
        return domain;
    }

    /// <summary>Backdate moves the registration date and leaves the expiry alone.</summary>
    [Fact]
    public void Backdate_ShouldMoveTheRegistrationDateOnly()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMonths(3);
        var domain = RegisteredDomain.Register(1, "example.com", expiresAt, true, false);
        var registeredAt = new DateTimeOffset(2025, 12, 1, 9, 0, 0, TimeSpan.Zero);

        domain.Backdate(registeredAt);

        domain.RegisteredAt.Should().Be(registeredAt);
        domain.ExpiresAt.Should().Be(expiresAt);
    }
}
