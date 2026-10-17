using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Domain registrar for the public demo: every operation succeeds at once, deterministically, with
/// no network. References are derived from the domain name, so the same name always gets the same
/// reference, and every name is available except the ones that spell <c>taken</c>.
/// </summary>
internal sealed class DemoRegistrarProvider : IRegistrarProvider
{
    /// <summary>A name containing this word reads as already registered, so the demo can show both answers.</summary>
    private const string TakenMarker = "taken";

    /// <summary>The registration term, in years, the fake registry grants on every order.</summary>
    private const int TermYears = 1;

    /// <summary>The sample contact the fake registry returns for any domain.</summary>
    private static readonly DomainContact SampleContact = new(
        "Demo", "Registrant", "Demo Registrant LLC", "registrant@demo.example", "+374 10 555 000",
        "1 Demo Street", null, "Yerevan", "Yerevan", "0001", "AM");

    /// <summary>Builds the reference the registry would hand back for a name.</summary>
    /// <param name="domainName">The domain name.</param>
    /// <returns>A stable reference.</returns>
    private static string RefFor(string domainName) => $"DEMO-{domainName.Trim().ToLowerInvariant()}";

    /// <summary>A successful result that carries the reference and, for orders, the new expiry.</summary>
    /// <param name="registrarRef">The registry reference.</param>
    /// <param name="withExpiry">Whether the result carries an expiry one term from now.</param>
    /// <returns>The success result.</returns>
    private static Task<RegistrarResult> Ok(string registrarRef, bool withExpiry = false) =>
        Task.FromResult(new RegistrarResult(
            true, registrarRef, withExpiry ? DateTimeOffset.UtcNow.AddYears(TermYears) : null, null));

    /// <inheritdoc/>
    public Task<RegistrarResult> RegisterAsync(DomainRegistrationOrder request, CancellationToken ct) =>
        Ok(RefFor(request.DomainName), true);

    /// <inheritdoc/>
    public Task<RegistrarResult> TransferAsync(DomainTransferOrder request, CancellationToken ct) =>
        Ok(RefFor(request.DomainName), true);

    /// <inheritdoc/>
    public Task<RegistrarResult> RenewAsync(DomainRenewalOrder request, CancellationToken ct) =>
        Ok(request.RegistrarRef, true);

    /// <inheritdoc/>
    public Task<RegistrarResult> CancelTransferAsync(string domainName, string registrarRef, CancellationToken ct) =>
        Ok(registrarRef);

    /// <inheritdoc/>
    public Task<RegistrarResult> InitiateOutgoingTransferAsync(string domainName, string registrarRef, CancellationToken ct) =>
        Ok(registrarRef);

    /// <inheritdoc/>
    public Task<RegistrarResult> SetAutoRenewAsync(string domainName, string registrarRef, bool enabled, CancellationToken ct) =>
        Ok(registrarRef);

    /// <inheritdoc/>
    public Task<RegistrarResult> SetWhoisPrivacyAsync(string domainName, string registrarRef, bool enabled, CancellationToken ct) =>
        Ok(registrarRef);

    /// <inheritdoc/>
    public Task<RegistrarResult> SetRegistrarLockAsync(string domainName, string registrarRef, bool locked, CancellationToken ct) =>
        Ok(registrarRef);

    /// <inheritdoc/>
    public Task<string?> GetEppCodeAsync(string domainName, string registrarRef, CancellationToken ct) =>
        Task.FromResult<string?>("DEMO-EPP-CODE");

    /// <inheritdoc/>
    public Task<RegistrarResult> SetNameserversAsync(
        string domainName, string registrarRef, IReadOnlyList<string> nameservers, CancellationToken ct) =>
        Ok(registrarRef);

    /// <inheritdoc/>
    public Task<IReadOnlyList<DnsRecord>> GetDnsRecordsAsync(string domainName, string registrarRef, CancellationToken ct)
    {
        IReadOnlyList<DnsRecord> records =
        [
            DnsRecord.CreateDetached(DnsRecordType.A, "@", "192.0.2.10", 3600, null),
            DnsRecord.CreateDetached(DnsRecordType.CNAME, "www", domainName, 3600, null),
            DnsRecord.CreateDetached(DnsRecordType.MX, "@", $"mail.{domainName}", 3600, 10),
        ];

        return Task.FromResult(records);
    }

    /// <inheritdoc/>
    public Task<RegistrarResult> AddDnsRecordAsync(string domainName, string registrarRef, DnsRecord record, CancellationToken ct) =>
        Ok(registrarRef);

    /// <inheritdoc/>
    public Task<RegistrarResult> UpdateDnsRecordAsync(string domainName, string registrarRef, DnsRecord record, CancellationToken ct) =>
        Ok(registrarRef);

    /// <inheritdoc/>
    public Task<RegistrarResult> DeleteDnsRecordAsync(string domainName, string registrarRef, int recordId, CancellationToken ct) =>
        Ok(registrarRef);

    /// <inheritdoc/>
    public Task<RegistrarResult> ModifyContactDetailsAsync(string domainName, DomainContact contact, CancellationToken ct) =>
        Ok(RefFor(domainName));

    /// <inheritdoc/>
    public Task<DomainContact?> GetContactDetailsAsync(string domainName, CancellationToken ct) =>
        Task.FromResult<DomainContact?>(SampleContact);

    /// <inheritdoc/>
    public Task<RegistrarResult> SetEmailForwardingAsync(string domainName, bool enabled, CancellationToken ct) =>
        Ok(RefFor(domainName));

    /// <inheritdoc/>
    public Task<RegistrarResult> AddEmailForwardingRuleAsync(string domainName, string source, string destination, CancellationToken ct) =>
        Ok(RefFor(domainName));

    /// <inheritdoc/>
    public Task<RegistrarResult> UpdateEmailForwardingRuleAsync(string domainName, string source, string destination, CancellationToken ct) =>
        Ok(RefFor(domainName));

    /// <inheritdoc/>
    public Task<RegistrarResult> DeleteEmailForwardingRuleAsync(string domainName, string source, CancellationToken ct) =>
        Ok(RefFor(domainName));

    /// <inheritdoc/>
    public Task<RegistrarResult> SetDnsManagementAsync(string domainName, bool enabled, CancellationToken ct) =>
        Ok(RefFor(domainName));

    /// <inheritdoc/>
    public Task<bool> CheckAvailabilityAsync(string domainName, CancellationToken ct) =>
        Task.FromResult(!domainName.Contains(TakenMarker, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc/>
    public Task<RegistrarResult> CheckDomainActiveAsync(string domainName, CancellationToken ct) =>
        Ok(RefFor(domainName), true);

    /// <inheritdoc/>
    public Task<WhoisInfo?> GetWhoisAsync(string domainName, CancellationToken ct)
    {
        var created = DateTimeOffset.UtcNow.AddMonths(-3);
        return Task.FromResult<WhoisInfo?>(new WhoisInfo("Demo Registrar", "Demo Registrant LLC", created, created.AddYears(TermYears)));
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<TldPricing>> GetTldPricingAsync(CancellationToken ct)
    {
        IReadOnlyList<TldPricing> pricing =
        [
            new TldPricing(
                "com", "USD",
                new Dictionary<int, decimal> { [1] = 9.6m },
                new Dictionary<int, decimal> { [1] = 9.6m },
                new Dictionary<int, decimal> { [1] = 12m }),
        ];

        return Task.FromResult(pricing);
    }
}
