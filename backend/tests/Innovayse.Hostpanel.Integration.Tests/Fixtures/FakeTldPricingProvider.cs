using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="IRegistrarProvider"/> standing in for a real registrar in the TLD import and
/// sync tests: every member but <see cref="GetTldPricingAsync"/> answers a trivial success no
/// test here reads, and the pricing catalogue a test sees is whatever it assigned to
/// <see cref="PricingToReturn"/> beforehand. A fake port, not a mock of our own code: the real
/// registrar is an upstream HTTP service this backend does not own.
/// </summary>
public sealed class FakeTldPricingProvider : IRegistrarProvider
{
    /// <summary>The catalogue <see cref="GetTldPricingAsync"/> answers with. Empty until a test sets it.</summary>
    public IReadOnlyList<TldPricing> PricingToReturn { get; set; } = [];

    /// <summary>How many times <see cref="GetTldPricingAsync"/> was called.</summary>
    public int PricingCallCount { get; private set; }

    /// <inheritdoc/>
    public Task<RegistrarResult> RegisterAsync(DomainRegistrationOrder request, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, "FAKE-REF", DateTimeOffset.UtcNow.AddYears(1), null));

    /// <inheritdoc/>
    public Task<RegistrarResult> TransferAsync(DomainTransferOrder request, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, "FAKE-TRANSFER-REF", DateTimeOffset.UtcNow.AddYears(1), null));

    /// <inheritdoc/>
    public Task<RegistrarResult> RenewAsync(DomainRenewalOrder request, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, "FAKE-RENEW-REF", DateTimeOffset.UtcNow.AddYears(1), null));

    /// <inheritdoc/>
    public Task<RegistrarResult> CancelTransferAsync(string domainName, string registrarRef, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> InitiateOutgoingTransferAsync(string domainName, string registrarRef, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> SetAutoRenewAsync(string domainName, string registrarRef, bool enabled, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> SetWhoisPrivacyAsync(string domainName, string registrarRef, bool enabled, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> SetRegistrarLockAsync(string domainName, string registrarRef, bool locked, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<string?> GetEppCodeAsync(string domainName, string registrarRef, CancellationToken ct) =>
        Task.FromResult<string?>("FAKE-EPP");

    /// <inheritdoc/>
    public Task<RegistrarResult> SetNameserversAsync(
        string domainName, string registrarRef, IReadOnlyList<string> nameservers, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<IReadOnlyList<DnsRecord>> GetDnsRecordsAsync(string domainName, string registrarRef, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<DnsRecord>>([]);

    /// <inheritdoc/>
    public Task<RegistrarResult> AddDnsRecordAsync(string domainName, string registrarRef, DnsRecord record, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> UpdateDnsRecordAsync(string domainName, string registrarRef, DnsRecord record, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> DeleteDnsRecordAsync(string domainName, string registrarRef, int recordId, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> ModifyContactDetailsAsync(string domainName, DomainContact contact, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, "FAKE-REF", null, null));

    /// <inheritdoc/>
    public Task<DomainContact?> GetContactDetailsAsync(string domainName, CancellationToken ct) =>
        Task.FromResult<DomainContact?>(null);

    /// <inheritdoc/>
    public Task<RegistrarResult> SetEmailForwardingAsync(string domainName, bool enabled, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, "FAKE-REF", null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> AddEmailForwardingRuleAsync(string domainName, string source, string destination, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, "FAKE-REF", null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> UpdateEmailForwardingRuleAsync(string domainName, string source, string destination, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, "FAKE-REF", null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> DeleteEmailForwardingRuleAsync(string domainName, string source, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, "FAKE-REF", null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> SetDnsManagementAsync(string domainName, bool enabled, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, "FAKE-REF", null, null));

    /// <inheritdoc/>
    public Task<bool> CheckAvailabilityAsync(string domainName, CancellationToken ct) => Task.FromResult(true);

    /// <inheritdoc/>
    public Task<RegistrarResult> CheckDomainActiveAsync(string domainName, CancellationToken ct) =>
        Task.FromResult(new RegistrarResult(true, domainName, DateTimeOffset.UtcNow.AddYears(1), null));

    /// <inheritdoc/>
    public Task<WhoisInfo?> GetWhoisAsync(string domainName, CancellationToken ct) => Task.FromResult<WhoisInfo?>(null);

    /// <inheritdoc/>
    public Task<IReadOnlyList<TldPricing>> GetTldPricingAsync(CancellationToken ct)
    {
        PricingCallCount++;
        return Task.FromResult(PricingToReturn);
    }
}
