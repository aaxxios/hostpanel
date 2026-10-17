using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;
/// <summary>
/// Test stub for <see cref="IRegistrarProvider"/>.
/// Returns hardcoded success results without making any real network calls.
/// </summary>
internal sealed class StubRegistrarProvider : IRegistrarProvider
{
    /// <summary>Every name <see cref="GetWhoisAsync"/> was asked about, in call order; the stub is scoped, so the record is static.</summary>
    public static readonly System.Collections.Concurrent.ConcurrentQueue<string> WhoisLookups = new();

    /// <inheritdoc/>
    public Task<RegistrarResult> RegisterAsync(DomainRegistrationOrder request, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, "STUB-REF-123", DateTimeOffset.UtcNow.AddYears(1), null));

    /// <inheritdoc/>
    public Task<RegistrarResult> TransferAsync(DomainTransferOrder request, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, "STUB-TRANSFER-456", DateTimeOffset.UtcNow.AddYears(1), null));

    /// <inheritdoc/>
    public Task<RegistrarResult> RenewAsync(DomainRenewalOrder request, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, "STUB-RENEW-789", DateTimeOffset.UtcNow.AddYears(1), null));

    /// <inheritdoc/>
    public Task<RegistrarResult> CancelTransferAsync(string domainName, string registrarRef, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> InitiateOutgoingTransferAsync(string domainName, string registrarRef, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> SetAutoRenewAsync(string domainName, string registrarRef, bool enabled, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> SetWhoisPrivacyAsync(string domainName, string registrarRef, bool enabled, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> SetRegistrarLockAsync(string domainName, string registrarRef, bool locked, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<string?> GetEppCodeAsync(string domainName, string registrarRef, CancellationToken ct)
        => Task.FromResult<string?>("STUB-EPP-CODE");

    /// <inheritdoc/>
    public Task<RegistrarResult> SetNameserversAsync(
        string domainName,
        string registrarRef,
        IReadOnlyList<string> nameservers,
        CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<IReadOnlyList<DnsRecord>> GetDnsRecordsAsync(
        string domainName,
        string registrarRef,
        CancellationToken ct)
        => Task.FromResult<IReadOnlyList<DnsRecord>>([]);

    /// <inheritdoc/>
    public Task<RegistrarResult> AddDnsRecordAsync(
        string domainName,
        string registrarRef,
        DnsRecord record,
        CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> UpdateDnsRecordAsync(
        string domainName,
        string registrarRef,
        DnsRecord record,
        CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> DeleteDnsRecordAsync(
        string domainName,
        string registrarRef,
        int recordId,
        CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, registrarRef, null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> ModifyContactDetailsAsync(string domainName, DomainContact contact, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, "STUB-REF", null, null));

    /// <summary>The TLD suffix under which the stub models a name the registrar holds no contact for.</summary>
    public const string NoContactSuffix = ".test";

    /// <summary>The registrant contact the stub answers with for every name outside <see cref="NoContactSuffix"/>.</summary>
    public static readonly DomainContact StubContact = new(
        "Stub", "Registrant", "Stub Org", "registrant@example.test", "+37400000000",
        "1 Registry St", null, "Yerevan", "Yerevan", "0001", "AM");

    /// <inheritdoc/>
    public Task<DomainContact?> GetContactDetailsAsync(string domainName, CancellationToken ct)
        => Task.FromResult<DomainContact?>(
            domainName.EndsWith(NoContactSuffix, StringComparison.OrdinalIgnoreCase) ? null : StubContact);

    /// <inheritdoc/>
    public Task<RegistrarResult> SetEmailForwardingAsync(string domainName, bool enabled, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, "STUB-REF", null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> AddEmailForwardingRuleAsync(string domainName, string source, string destination, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, "STUB-REF", null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> UpdateEmailForwardingRuleAsync(string domainName, string source, string destination, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, "STUB-REF", null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> DeleteEmailForwardingRuleAsync(string domainName, string source, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, "STUB-REF", null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> SetDnsManagementAsync(string domainName, bool enabled, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, "STUB-REF", null, null));

    /// <inheritdoc/>
    public Task<bool> CheckAvailabilityAsync(string domainName, CancellationToken ct)
        => Task.FromResult(true);

    /// <inheritdoc/>
    public Task<WhoisInfo?> GetWhoisAsync(string domainName, CancellationToken ct)
    {
        WhoisLookups.Enqueue(domainName);
        return Task.FromResult<WhoisInfo?>(null);
    }

    /// <inheritdoc/>
    public Task<RegistrarResult> CheckDomainActiveAsync(string domainName, CancellationToken ct)
        => Task.FromResult(new RegistrarResult(true, domainName, DateTimeOffset.UtcNow.AddYears(1), null));

    /// <inheritdoc/>
    public Task<IReadOnlyList<TldPricing>> GetTldPricingAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<TldPricing>>([]);
}
