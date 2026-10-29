using System.Xml.Linq;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap;

/// <summary>
/// Implements <see cref="IRegistrarProvider"/> using the Namecheap XML API v2.
/// All operations are dispatched through <see cref="NamecheapClient"/>.
/// </summary>
public sealed class NamecheapRegistrarProvider : IRegistrarProvider
{
    /// <summary>The four contact roles Namecheap's create and set-contacts commands take; each is sent the same contact.</summary>
    private static readonly string[] ContactRoles = ["Registrant", "Tech", "Admin", "AuxBilling"];

    /// <summary>The Namecheap XML API client every registrar call goes through.</summary>
    private readonly NamecheapClient _client;

    /// <summary>Where the registrar's own refusal wording goes; the caller only sees an error code.</summary>
    private readonly ILogger<NamecheapRegistrarProvider> _logger;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes a new instance of the <see cref="NamecheapRegistrarProvider"/> class.</summary>
    /// <param name="client">The Namecheap XML API client every registrar call goes through.</param>
    /// <param name="logger">Where the registrar's own refusal wording goes.</param>
    /// <param name="clock">The source of the current moment.</param>
    public NamecheapRegistrarProvider(NamecheapClient client, ILogger<NamecheapRegistrarProvider> logger, IClock clock)
    {
        _client = client;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>
    /// The answer to a purchase or a change when the registrar is not configured: a refusal, never
    /// a no-op success. A paid order is then refunded, a paid renewal retried, and a handler does
    /// not save a change the registrar never made. Domains not held at a registrar never get here:
    /// the handlers skip the registrar for them.
    /// </summary>
    private static readonly RegistrarResult NotConfiguredResult =
        new(false, null, null, HostpanelErrorCodes.REGISTRAR_NOT_CONFIGURED);

    /// <inheritdoc/>
    public async Task<RegistrarResult> RegisterAsync(DomainRegistrationOrder request, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        if (!request.Contact.IsCompleteForPurchase())
        {
            _logger.LogWarning("The registrant contact for '{DomainName}' is incomplete; nothing was ordered", request.DomainName);
            return new RegistrarResult(false, null, null, HostpanelErrorCodes.REGISTRAR_CONTACT_VALIDATION_FAILED);
        }

        var (sld, tld) = RegistrarDomainName.Split(request.DomainName);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
            ["Years"] = request.Years.ToString(),
            ["AddFreeWhoisguard"] = request.WhoisPrivacy ? "yes" : "no",
            ["WGEnabled"] = request.WhoisPrivacy ? "yes" : "no",
        };

        if (request.Nameserver1 is not null)
        {
            parameters["Nameservers"] = BuildNameserverString(request.Nameserver1, request.Nameserver2);
        }

        AddContactParameters(parameters, request.Contact);

        var (doc, refusalCode) = await ExecutePurchaseAsync(new NamecheapCommand("namecheap.domains.create", parameters), ct);
        if (refusalCode is not null)
        {
            return await SettleRefusalAsync(request.DomainName, refusalCode, includeTransfers: false, ct);
        }

        var result = ConfirmedResult(doc!, "DomainCreateResult", "Registered");
        if (result is null)
        {
            _logger.LogWarning("Namecheap did not confirm the registration of '{DomainName}'", request.DomainName);
            return new RegistrarResult(false, null, null, HostpanelErrorCodes.REGISTRAR_ORDER_UPSTREAM_FAILED);
        }

        var expiresAt = RegistrarDate.Parse(result.Attribute("ExpireDate")?.Value)
            ?? _clock.UtcNow.AddYears(request.Years);

        return new RegistrarResult(true, ReferenceOrName(result, "DomainID", request.DomainName), expiresAt, null);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> TransferAsync(DomainTransferOrder request, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        var parameters = new Dictionary<string, string>
        {
            ["DomainName"] = request.DomainName,
            ["EPPCode"] = request.EppCode,
            ["AddFreeWhoisguard"] = request.WhoisPrivacy ? "yes" : "no",
        };

        var (doc, refusalCode) = await ExecutePurchaseAsync(
            new NamecheapCommand("namecheap.domains.transfer.create", parameters), ct);
        if (refusalCode is not null)
        {
            return await SettleRefusalAsync(request.DomainName, refusalCode, includeTransfers: true, ct);
        }

        var result = ConfirmedResult(doc!, "DomainTransferCreateResult", "Transfer");
        if (result is null)
        {
            _logger.LogWarning("Namecheap did not confirm the transfer of '{DomainName}'", request.DomainName);
            return new RegistrarResult(false, null, null, HostpanelErrorCodes.REGISTRAR_ORDER_UPSTREAM_FAILED);
        }

        var expiresAt = RegistrarDate.Parse(result.Attribute("ExpireDate")?.Value);

        return new RegistrarResult(true, ReferenceOrName(result, "TransferID", request.DomainName), expiresAt, null);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Unlike a registration or a transfer, a refused renewal is never turned into success because
    /// the account holds the domain: holding it says nothing about whether it was renewed.
    /// </remarks>
    public async Task<RegistrarResult> RenewAsync(DomainRenewalOrder request, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        var (sld, tld) = RegistrarDomainName.Split(request.DomainName);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
            ["Years"] = request.Years.ToString(),
        };

        var (doc, refusalCode) = await ExecutePurchaseAsync(new NamecheapCommand("namecheap.domains.renew", parameters), ct);
        if (refusalCode is not null)
        {
            return new RegistrarResult(false, null, null, refusalCode);
        }

        var result = ConfirmedResult(doc!, "DomainRenewResult", "Renew");
        if (result is null)
        {
            _logger.LogWarning("Namecheap did not confirm the renewal of '{DomainName}'", request.DomainName);
            return new RegistrarResult(false, null, null, HostpanelErrorCodes.REGISTRAR_ORDER_UPSTREAM_FAILED);
        }

        // No expiry in the answer leaves it to the caller to extend the current one; now + years
        // would cost an early renewal its remaining days.
        var expiresAt = RegistrarDate.Parse(result.Attribute("ExpireDate")?.Value);

        return new RegistrarResult(true, request.RegistrarRef, expiresAt, null);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The registrar exposes no cancel-transfer endpoint, so an in-progress transfer can only be
    /// cancelled through its support; this always answers a failure.
    /// </remarks>
    public Task<RegistrarResult> CancelTransferAsync(DomainRef domain, CancellationToken ct)
    {
        return Task.FromResult(new RegistrarResult(false, domain.RegistrarRef, null,
            HostpanelErrorCodes.REGISTRAR_OPERATION_UNSUPPORTED));
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> InitiateOutgoingTransferAsync(DomainRef domain, CancellationToken ct)
    {
        // Unlock the domain to permit outgoing transfer.
        return await SetRegistrarLockAsync(new RegistrarFlagChange(domain, false), ct);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> SetAutoRenewAsync(RegistrarFlagChange change, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        var (sld, tld) = RegistrarDomainName.Split(change.Domain.Name);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
            ["IsAutoRenew"] = change.Enabled ? "true" : "false",
        };

        return await ChangeAsync(
            "switching auto-renew", new NamecheapCommand("namecheap.domains.setAutoRenew", parameters), change.Domain.RegistrarRef, ct);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> SetWhoisPrivacyAsync(RegistrarFlagChange change, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        return await GuardAsync("switching WHOIS privacy", async () =>
        {
            // Step 1: Get the WhoisguardID from domain info
            var infoParams = new Dictionary<string, string>
            {
                ["DomainName"] = change.Domain.Name,
            };

            var infoDoc = await _client.ExecuteAsync(new NamecheapCommand("namecheap.domains.getInfo", infoParams), ct);
            var ns = GetNamespace(infoDoc);

            var whoisguardNode = infoDoc.Root
                ?.Element(XName.Get("CommandResponse", ns))
                ?.Element(XName.Get("DomainGetInfoResult", ns))
                ?.Element(XName.Get("Whoisguard", ns));

            var whoisguardId = whoisguardNode?.Element(XName.Get("ID", ns))?.Value;

            if (string.IsNullOrEmpty(whoisguardId))
            {
                _logger.LogWarning(
                    "Namecheap holds no WhoisGuard subscription for '{DomainName}'", change.Domain.Name);
                return new RegistrarResult(false, change.Domain.RegistrarRef, null,
                    HostpanelErrorCodes.REGISTRAR_WHOIS_PRIVACY_UNAVAILABLE);
            }

            // Step 2: Enable or disable WhoisGuard using the retrieved ID
            var command = change.Enabled ? "namecheap.whoisguard.enable" : "namecheap.whoisguard.disable";
            var parameters = new Dictionary<string, string>
            {
                ["WhoisguardID"] = whoisguardId,
                ["ForwardedToEmail"] = string.Empty,
            };

            await _client.ExecuteAsync(new NamecheapCommand(command, parameters), ct);
            return new RegistrarResult(true, change.Domain.RegistrarRef, null, null);
        }, ct);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> SetRegistrarLockAsync(RegistrarFlagChange change, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        var (sld, tld) = RegistrarDomainName.Split(change.Domain.Name);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
            ["LockAction"] = change.Enabled ? "LOCK" : "UNLOCK",
        };

        return await ChangeAsync(
            "switching the registrar lock", new NamecheapCommand("namecheap.domains.setRegistrarLock", parameters), change.Domain.RegistrarRef, ct);
    }

    /// <inheritdoc/>
    public async Task<string?> GetEppCodeAsync(DomainRef domain, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return null;
        }

        var (sld, tld) = RegistrarDomainName.Split(domain.Name);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
        };

        var doc = await _client.ExecuteAsync(new NamecheapCommand("namecheap.domains.getEPPCode", parameters), ct);

        var ns = GetNamespace(doc);
        var result = doc.Root
            ?.Element(XName.Get("CommandResponse", ns))
            ?.Element(XName.Get("DomainGetEPPCodeResult", ns));

        return result?.Attribute("EPPCode")?.Value ?? result?.Value;
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> SetNameserversAsync(NameserverChange change, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        var (sld, tld) = RegistrarDomainName.Split(change.Domain.Name);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
            ["Nameservers"] = string.Join(",", change.Nameservers),
        };

        return await ChangeAsync(
            "setting nameservers", new NamecheapCommand("namecheap.domains.dns.setCustom", parameters), change.Domain.RegistrarRef, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DnsRecord>> GetDnsRecordsAsync(DomainRef domain, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return [];
        }

        var (sld, tld) = RegistrarDomainName.Split(domain.Name);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
        };

        var doc = await _client.ExecuteAsync(new NamecheapCommand("namecheap.domains.dns.getHosts", parameters), ct);

        var ns = GetNamespace(doc);
        var hostNodes = doc.Root
            ?.Element(XName.Get("CommandResponse", ns))
            ?.Element(XName.Get("DomainDNSGetHostsResult", ns))
            ?.Elements(XName.Get("host", ns))
            ?? [];

        var records = new List<DnsRecord>();

        foreach (var host in hostNodes)
        {
            var typeRaw = host.Attribute("Type")?.Value ?? "A";
            if (!Enum.TryParse<DnsRecordType>(typeRaw, true, out var recordType))
            {
                continue;
            }

            var hostName = host.Attribute("Name")?.Value ?? "@";
            var address = host.Attribute("Address")?.Value ?? string.Empty;
            var ttlRaw = host.Attribute("TTL")?.Value ?? "1800";
            var ttl = int.TryParse(ttlRaw, out var parsedTtl) ? parsedTtl : 1800;
            var mxPrefRaw = host.Attribute("MXPref")?.Value;
            int? priority = int.TryParse(mxPrefRaw, out var pref) ? pref : null;

            records.Add(DnsRecord.CreateDetached(recordType, hostName, address, ttl, priority));
        }

        return records.AsReadOnly();
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> AddDnsRecordAsync(DnsRecordChange change, CancellationToken ct)
    {
        return await GuardAsync("adding a DNS record", async () =>
        {
            var existing = await GetDnsRecordsAsync(change.Domain, ct);
            var updated = existing.ToList();
            updated.Add(change.Record);
            return await SetAllHostsAsync(change.Domain.Name, change.Domain.RegistrarRef, updated, ct);
        }, ct);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> UpdateDnsRecordAsync(DnsRecordChange change, CancellationToken ct)
    {
        return await GuardAsync("updating a DNS record", async () =>
        {
            var existing = await GetDnsRecordsAsync(change.Domain, ct);

            var updated = existing
                .Select(r => r.Host == change.Record.Host && r.Type == change.Record.Type ? change.Record : r)
                .ToList();

            return await SetAllHostsAsync(change.Domain.Name, change.Domain.RegistrarRef, updated, ct);
        }, ct);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> DeleteDnsRecordAsync(DnsRecordRemoval removal, CancellationToken ct)
    {
        return await GuardAsync("deleting a DNS record", async () =>
        {
            var existing = await GetDnsRecordsAsync(removal.Domain, ct);

            // Namecheap has no native record IDs: skip the record at the given 1-based position index.
            var updated = existing
                .Where((_, idx) => idx + 1 != removal.RecordId)
                .ToList();

            return await SetAllHostsAsync(removal.Domain.Name, removal.Domain.RegistrarRef, updated, ct);
        }, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> CheckAvailabilityAsync(string domainName, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            // Not "taken": unknown. A change is refused with a failed result, but a yes/no
            // question has no such answer: a storefront that reports every domain as taken
            // sells nothing and looks like it is working. Surfacing it means an operator who forgot to
            // configure a registrar finds out, instead of quietly selling nothing.
            throw new InvalidOperationException(
                "Domain availability cannot be checked because no registrar is configured. "
                + "Set the registrar credentials in Admin -> Integrations.");
        }

        var parameters = new Dictionary<string, string>
        {
            ["DomainList"] = domainName,
        };

        var doc = await _client.ExecuteAsync(new NamecheapCommand("namecheap.domains.check", parameters), ct);

        var ns = GetNamespace(doc);
        var result = doc.Root
            ?.Element(XName.Get("CommandResponse", ns))
            ?.Element(XName.Get("DomainCheckResult", ns));

        var available = result?.Attribute("Available")?.Value;
        return string.Equals(available, "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> ModifyContactDetailsAsync(DomainContactChange change, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        var (sld, tld) = RegistrarDomainName.Split(change.DomainName);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
        };

        // Namecheap's setContacts applies to all four contact roles in one call.
        // We have no per-role data, so the same contact is used for every role:
        // this matches how NameAm's ModifyContactDetailsAsync treats the same input.
        AddContactParameters(parameters, change.Contact);

        return await ChangeAsync(
            "changing contact details", new NamecheapCommand("namecheap.domains.setContacts", parameters), null, ct);
    }

    /// <inheritdoc/>
    public async Task<DomainContact?> GetContactDetailsAsync(string domainName, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return null;
        }

        var (sld, tld) = RegistrarDomainName.Split(domainName);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
        };

        var doc = await _client.ExecuteAsync(new NamecheapCommand("namecheap.domains.getContacts", parameters), ct);

        var ns = GetNamespace(doc);
        var registrant = doc.Root
            ?.Element(XName.Get("CommandResponse", ns))
            ?.Element(XName.Get("DomainContactsResult", ns))
            ?.Element(XName.Get("Registrant", ns));

        if (registrant is null)
        {
            return null;
        }

        string Read(string field) => registrant.Element(XName.Get(field, ns))?.Value ?? string.Empty;

        var organization = Read("OrganizationName");
        var address2 = Read("Address2");

        return new DomainContact(
            Read("FirstName"),
            Read("LastName"),
            organization.Length == 0 ? null : organization,
            Read("EmailAddress"),
            Read("Phone"),
            Read("Address1"),
            address2.Length == 0 ? null : address2,
            Read("City"),
            Read("StateProvince"),
            Read("PostalCode"),
            Read("Country"));
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> SetEmailForwardingAsync(DomainFeatureToggle toggle, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        // Namecheap has no dedicated on/off switch: email forwarding is implicit
        // in whether any forwarding rules exist. Disabling clears every rule;
        // enabling with none configured yet is a no-op until rules are added.
        if (!toggle.Enabled)
        {
            return await SetAllEmailForwardsAsync(toggle.DomainName, [], ct);
        }

        return new RegistrarResult(true, null, null, null);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> AddEmailForwardingRuleAsync(EmailForwardingRuleChange change, CancellationToken ct)
    {
        return await GuardAsync("adding an email forwarding rule", async () =>
        {
            var existing = await GetEmailForwardsAsync(change.DomainName, ct);
            var updated = existing
                .Where(r => !string.Equals(r.Mailbox, change.Source, StringComparison.OrdinalIgnoreCase))
                .Append(new NamecheapEmailForward(change.Source, change.Destination))
                .ToList();

            return await SetAllEmailForwardsAsync(change.DomainName, updated, ct);
        }, ct);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> UpdateEmailForwardingRuleAsync(EmailForwardingRuleChange change, CancellationToken ct)
    {
        return await GuardAsync("updating an email forwarding rule", async () =>
        {
            var existing = await GetEmailForwardsAsync(change.DomainName, ct);
            var updated = existing
                .Select(r => string.Equals(r.Mailbox, change.Source, StringComparison.OrdinalIgnoreCase)
                    ? new NamecheapEmailForward(change.Source, change.Destination)
                    : r)
                .ToList();

            return await SetAllEmailForwardsAsync(change.DomainName, updated, ct);
        }, ct);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> DeleteEmailForwardingRuleAsync(EmailForwardingRuleRemoval removal, CancellationToken ct)
    {
        return await GuardAsync("deleting an email forwarding rule", async () =>
        {
            var existing = await GetEmailForwardsAsync(removal.DomainName, ct);
            var updated = existing
                .Where(r => !string.Equals(r.Mailbox, removal.Source, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return await SetAllEmailForwardsAsync(removal.DomainName, updated, ct);
        }, ct);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> SetDnsManagementAsync(DomainFeatureToggle toggle, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        // "DNS management" here means "let Namecheap host the DNS": enabling it
        // repoints the domain at Namecheap's own nameservers. Disabling has no
        // corresponding API call; the caller is expected to point nameservers
        // elsewhere via SetNameserversAsync instead.
        if (!toggle.Enabled)
        {
            return new RegistrarResult(true, null, null, null);
        }

        var (sld, tld) = RegistrarDomainName.Split(toggle.DomainName);
        var parameters = new Dictionary<string, string> { ["SLD"] = sld, ["TLD"] = tld };

        return await ChangeAsync(
            "switching DNS management", new NamecheapCommand("namecheap.domains.dns.setDefault", parameters), null, ct);
    }

    /// <inheritdoc/>
    public async Task<WhoisInfo?> GetWhoisAsync(string domainName, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return null;
        }

        var (sld, tld) = RegistrarDomainName.Split(domainName);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
        };

        var doc = await _client.ExecuteAsync(new NamecheapCommand("namecheap.domains.getInfo", parameters), ct);

        var ns = GetNamespace(doc);
        var info = doc.Root
            ?.Element(XName.Get("CommandResponse", ns))
            ?.Element(XName.Get("DomainGetInfoResult", ns));

        if (info is null)
        {
            return null;
        }

        var registrar = info.Attribute("RegistrarName")?.Value ?? "Namecheap";
        var registrant = info
            .Element(XName.Get("Registrant", ns))
            ?.Element(XName.Get("OrganizationName", ns))
            ?.Value ?? string.Empty;

        var createdAt = RegistrarDate.Parse(info.Attribute("CreatedDate")?.Value) ?? DateTimeOffset.MinValue;
        var expiresAt = RegistrarDate.Parse(info.Attribute("ExpiredDate")?.Value) ?? DateTimeOffset.MaxValue;

        return new WhoisInfo(registrar, registrant, createdAt, expiresAt);
    }

    /// <inheritdoc/>
    public async Task<RegistrarResult> CheckDomainActiveAsync(string domainName, CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        var (sld, tld) = RegistrarDomainName.Split(domainName);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
        };

        return await GuardAsync("checking a domain's status", async () =>
        {
            var doc = await _client.ExecuteAsync(new NamecheapCommand("namecheap.domains.getInfo", parameters), ct);
            var ns = GetNamespace(doc);
            var info = doc.Root
                ?.Element(XName.Get("CommandResponse", ns))
                ?.Element(XName.Get("DomainGetInfoResult", ns));

            if (info is null)
            {
                return new RegistrarResult(false, null, null, HostpanelErrorCodes.REGISTRAR_DOMAIN_NOT_FOUND);
            }

            var domainId = info.Attribute("ID")?.Value ?? domainName;
            var expiresAt = RegistrarDate.Parse(info.Attribute("ExpiredDate")?.Value) ?? _clock.UtcNow.AddYears(1);

            return new RegistrarResult(true, domainId, expiresAt, null);
        }, ct);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<TldPricing>> GetTldPricingAsync(CancellationToken ct)
    {
        // Namecheap does not expose a TLD pricing API in a compatible format.
        return Task.FromResult<IReadOnlyList<TldPricing>>([]);
    }

    /// <summary>
    /// Pushes the complete list of DNS host records to Namecheap using <c>namecheap.domains.dns.setHosts</c>.
    /// Namecheap requires the full record set on every write: partial updates are not supported.
    /// </summary>
    /// <param name="domainName">The fully-qualified domain name.</param>
    /// <param name="registrarRef">The registrar's reference for this domain.</param>
    /// <param name="records">Complete list of DNS records to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating whether the operation succeeded.</returns>
    private async Task<RegistrarResult> SetAllHostsAsync(
        string domainName,
        string registrarRef,
        IList<DnsRecord> records,
        CancellationToken ct)
    {
        // The one write behind adding, updating and deleting a record.
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        var (sld, tld) = RegistrarDomainName.Split(domainName);

        var parameters = new Dictionary<string, string>
        {
            ["SLD"] = sld,
            ["TLD"] = tld,
        };

        for (var i = 0; i < records.Count; i++)
        {
            var n = i + 1;
            var rec = records[i];
            parameters[$"HostName{n}"] = rec.Host;
            parameters[$"RecordType{n}"] = rec.Type.ToString();
            parameters[$"Address{n}"] = rec.Value;
            parameters[$"TTL{n}"] = rec.Ttl.ToString();

            if (rec.Priority.HasValue)
            {
                parameters[$"MXPref{n}"] = rec.Priority.Value.ToString();
            }
        }

        await _client.ExecuteAsync(new NamecheapCommand("namecheap.domains.dns.setHosts", parameters), ct);
        return new RegistrarResult(true, registrarRef, null, null);
    }

    /// <summary>
    /// Reads the current email forwarding rules via <c>namecheap.domains.dns.getEmailForwarding</c>.
    /// </summary>
    /// <param name="domainName">The fully-qualified domain name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The forwarding rules currently configured.</returns>
    private async Task<List<NamecheapEmailForward>> GetEmailForwardsAsync(
        string domainName,
        CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return [];
        }

        var parameters = new Dictionary<string, string> { ["DomainName"] = domainName };
        var doc = await _client.ExecuteAsync(new NamecheapCommand("namecheap.domains.dns.getEmailForwarding", parameters), ct);

        var ns = GetNamespace(doc);
        var forwardNodes = doc.Root
            ?.Element(XName.Get("CommandResponse", ns))
            ?.Element(XName.Get("DomainDNSGetEmailForwardingResult", ns))
            ?.Elements(XName.Get("Forward", ns))
            ?? [];

        var forwards = new List<NamecheapEmailForward>();
        foreach (var node in forwardNodes)
        {
            var mailbox = node.Attribute("mailbox")?.Value;
            var forwardTo = node.Value;
            if (!string.IsNullOrEmpty(mailbox) && !string.IsNullOrEmpty(forwardTo))
            {
                forwards.Add(new NamecheapEmailForward(mailbox, forwardTo));
            }
        }

        return forwards;
    }

    /// <summary>
    /// Pushes the complete set of email forwarding rules to Namecheap using
    /// <c>namecheap.domains.dns.setEmailForwarding</c>. As with DNS hosts, Namecheap
    /// requires the full rule set on every write: there is no add/remove-single-rule call.
    /// </summary>
    /// <param name="domainName">The fully-qualified domain name.</param>
    /// <param name="forwards">The complete list of forwarding rules to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating whether the operation succeeded.</returns>
    private async Task<RegistrarResult> SetAllEmailForwardsAsync(
        string domainName,
        IReadOnlyList<NamecheapEmailForward> forwards,
        CancellationToken ct)
    {
        if (!_client.IsConfigured)
        {
            return NotConfiguredResult;
        }

        var parameters = new Dictionary<string, string> { ["DomainName"] = domainName };

        for (var i = 0; i < forwards.Count; i++)
        {
            var n = i + 1;
            parameters[$"MailBox{n}"] = forwards[i].Mailbox;
            parameters[$"ForwardTo{n}"] = forwards[i].ForwardTo;
        }

        return await ChangeAsync(
            "setting email forwarding", new NamecheapCommand("namecheap.domains.dns.setEmailForwarding", parameters), null, ct);
    }

    /// <summary>
    /// Runs a command that changes something and is not a purchase, and answers success once
    /// Namecheap accepted it.
    /// </summary>
    /// <param name="operation">What is being done, for the log.</param>
    /// <param name="command">The command to run.</param>
    /// <param name="registrarRef">The registrar reference to hand back on success; <see langword="null"/> when there is none.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or the failed result when Namecheap did not make the change.</returns>
    private Task<RegistrarResult> ChangeAsync(
        string operation, NamecheapCommand command, string? registrarRef, CancellationToken ct)
    {
        return GuardAsync(operation, async () =>
        {
            await _client.ExecuteAsync(command, ct);
            return new RegistrarResult(true, registrarRef, null, null);
        }, ct);
    }

    /// <summary>
    /// Runs a change that is not a purchase and turns every way the registrar call can fail into a
    /// failed result, so the handler answers with a code instead of an exception carrying the
    /// registrar's wording. That wording is logged here and goes no further.
    /// </summary>
    /// <param name="operation">What is being done, for the log.</param>
    /// <param name="action">The change to run.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The change's result, or a failed one when the registrar call failed.</returns>
    private async Task<RegistrarResult> GuardAsync(string operation, Func<Task<RegistrarResult>> action, CancellationToken ct)
    {
        try
        {
            return await action();
        }
        catch (Exception ex) when (RegistrarFailure.IsUpstream(ex, ct))
        {
            _logger.LogWarning(ex, "Namecheap failed while {Operation}", operation);
            return new RegistrarResult(
                false,
                null,
                null,
                ex is RegistrarException refusal ? RefusalCodeFor(refusal) : HostpanelErrorCodes.REGISTRAR_ORDER_UPSTREAM_FAILED);
        }
    }

    /// <summary>
    /// Runs a command that buys something. An <c>ERROR</c> answer is Namecheap refusing it, so
    /// nothing was bought: it comes back as a refusal the caller can act on (refund, retry)
    /// rather than an exception it would have to treat as an unknown outcome.
    /// </summary>
    /// <param name="command">The Namecheap command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The response, or the refusal code when Namecheap answered <c>ERROR</c>.</returns>
    /// <exception cref="HttpRequestException">
    /// Thrown on a transport failure, a non-success status or an answer that is not XML: the
    /// outcome is unknown.
    /// </exception>
    private async Task<NamecheapPurchaseOutcome> ExecutePurchaseAsync(NamecheapCommand command, CancellationToken ct)
    {
        try
        {
            return new NamecheapPurchaseOutcome(await _client.ExecuteAsync(command, ct), null);
        }
        catch (RegistrarException ex)
        {
            _logger.LogWarning(ex, "Namecheap refused {Command}: {Error}", command.Name, ex.Message);
            return new NamecheapPurchaseOutcome(null, RefusalCodeFor(ex));
        }
    }

    /// <summary>
    /// Picks the error code for a refusal Namecheap sent: its two causes a caller can act on get
    /// their own code, everything else is the plain refusal.
    /// </summary>
    /// <param name="refusal">The refusal Namecheap answered with.</param>
    /// <returns>A <c>REGISTRAR_*</c> code.</returns>
    private static string RefusalCodeFor(RegistrarException refusal)
    {
        return refusal switch
        {
            InsufficientFundsException => HostpanelErrorCodes.REGISTRAR_INSUFFICIENT_FUNDS,
            DomainNotAvailableException => HostpanelErrorCodes.REGISTRAR_DOMAIN_UNAVAILABLE,
            _ => HostpanelErrorCodes.REGISTRAR_ORDER_UPSTREAM_FAILED,
        };
    }

    /// <summary>
    /// Settles a refused registration or transfer. A purchase whose answer was lost (a timeout, a
    /// 5xx) may have gone through, and its retry is then refused as "domain unavailable" because
    /// the account already holds the name; refusing that would refund a domain we own. So that one
    /// refusal is checked against the account first, and the order counts as placed when it holds
    /// the name. A failure to look escapes, so an unconfirmed refusal is never reported as one.
    /// </summary>
    /// <param name="domainName">The domain that was ordered.</param>
    /// <param name="refusalCode">The refusal code the purchase ended with.</param>
    /// <param name="includeTransfers">Whether a transfer still in progress counts as the order being placed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success when the account holds the domain, otherwise the refusal.</returns>
    private async Task<RegistrarResult> SettleRefusalAsync(
        string domainName, string refusalCode, bool includeTransfers, CancellationToken ct)
    {
        if (refusalCode == HostpanelErrorCodes.REGISTRAR_DOMAIN_UNAVAILABLE)
        {
            var placed = await FindPlacedOrderAsync(domainName, includeTransfers, ct);
            if (placed is not null)
            {
                _logger.LogWarning(
                    "Namecheap refused the order for '{DomainName}' but the account already holds it; counting it as placed",
                    domainName);
                return placed;
            }
        }

        return new RegistrarResult(false, null, null, refusalCode);
    }

    /// <summary>
    /// Looks for a domain in the account (<c>namecheap.domains.getList</c>), and for a transfer of
    /// it still in progress (<c>namecheap.domains.transfer.getList</c>) when asked.
    /// </summary>
    /// <param name="domainName">The domain that was ordered.</param>
    /// <param name="includeTransfers">Whether to look at transfers in progress too.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A success result for what is found, or <see langword="null"/> when the account holds nothing by that name.</returns>
    private async Task<RegistrarResult?> FindPlacedOrderAsync(string domainName, bool includeTransfers, CancellationToken ct)
    {
        var listParameters = new Dictionary<string, string> { ["SearchTerm"] = domainName };
        var listDoc = await _client.ExecuteAsync(new NamecheapCommand("namecheap.domains.getList", listParameters), ct);
        var ns = GetNamespace(listDoc);

        var held = listDoc.Root
            ?.Element(XName.Get("CommandResponse", ns))
            ?.Element(XName.Get("DomainGetListResult", ns))
            ?.Elements(XName.Get("Domain", ns))
            .FirstOrDefault(d => string.Equals(d.Attribute("Name")?.Value, domainName, StringComparison.OrdinalIgnoreCase));

        if (held is not null)
        {
            // The list gives the expiry as MM/dd/yyyy, which the invariant culture reads.
            return new RegistrarResult(
                true, ReferenceOrName(held, "ID", domainName), RegistrarDate.Parse(held.Attribute("Expires")?.Value), null);
        }

        if (!includeTransfers)
        {
            return null;
        }

        var transferParameters = new Dictionary<string, string> { ["ListType"] = "INPROGRESS", ["SearchTerm"] = domainName };
        var transferDoc = await _client.ExecuteAsync(
            new NamecheapCommand("namecheap.domains.transfer.getList", transferParameters), ct);
        var transferNs = GetNamespace(transferDoc);

        var inProgress = transferDoc.Root
            ?.Element(XName.Get("CommandResponse", transferNs))
            ?.Element(XName.Get("DomainTransferGetListResult", transferNs))
            ?.Elements(XName.Get("Transfer", transferNs))
            .Any(t => string.Equals(t.Attribute("DomainName")?.Value, domainName, StringComparison.OrdinalIgnoreCase));

        return inProgress == true ? new RegistrarResult(true, domainName, null, null) : null;
    }

    /// <summary>
    /// Adds a contact to a command's parameters once for each of the four roles Namecheap takes.
    /// </summary>
    /// <param name="parameters">The command's parameters; written to.</param>
    /// <param name="contact">The contact every role is given.</param>
    private static void AddContactParameters(Dictionary<string, string> parameters, DomainContact contact)
    {
        foreach (var role in ContactRoles)
        {
            parameters[$"{role}FirstName"] = contact.FirstName;
            parameters[$"{role}LastName"] = contact.LastName;
            parameters[$"{role}OrganizationName"] = contact.Organization ?? string.Empty;
            parameters[$"{role}EmailAddress"] = contact.Email;
            parameters[$"{role}Phone"] = contact.Phone;
            parameters[$"{role}Address1"] = contact.Address1;
            parameters[$"{role}Address2"] = contact.Address2 ?? string.Empty;
            parameters[$"{role}City"] = contact.City;
            parameters[$"{role}StateProvince"] = contact.State;
            parameters[$"{role}PostalCode"] = contact.PostalCode;
            parameters[$"{role}Country"] = contact.Country;
        }
    }

    /// <summary>
    /// Finds a command's result element when it confirms the purchase. An OK envelope is not proof
    /// on its own: only the confirming attribute set to <c>true</c> says it was bought.
    /// </summary>
    /// <param name="doc">The response.</param>
    /// <param name="resultElement">The result element name (e.g. <c>DomainRenewResult</c>).</param>
    /// <param name="confirmAttribute">The attribute that confirms it (e.g. <c>Renew</c>).</param>
    /// <returns>The result element, or <see langword="null"/> when it is missing or unconfirmed.</returns>
    private static XElement? ConfirmedResult(XDocument doc, string resultElement, string confirmAttribute)
    {
        var ns = GetNamespace(doc);
        var result = doc.Root
            ?.Element(XName.Get("CommandResponse", ns))
            ?.Element(XName.Get(resultElement, ns));

        return string.Equals(result?.Attribute(confirmAttribute)?.Value, "true", StringComparison.OrdinalIgnoreCase)
            ? result
            : null;
    }

    /// <summary>
    /// Reads Namecheap's reference for a purchase, or the domain name when it gave none: every
    /// later Namecheap call addresses the domain by its name, and activation refuses an empty reference.
    /// </summary>
    /// <param name="result">The confirmed result element.</param>
    /// <param name="attribute">The reference attribute (e.g. <c>DomainID</c>).</param>
    /// <param name="domainName">The domain name.</param>
    /// <returns>The reference to store.</returns>
    private static string ReferenceOrName(XElement result, string attribute, string domainName)
    {
        var reference = result.Attribute(attribute)?.Value;
        return string.IsNullOrWhiteSpace(reference) ? domainName : reference;
    }

    /// <summary>
    /// Extracts the default XML namespace URI from the root element of a Namecheap API response document.
    /// </summary>
    /// <param name="doc">The parsed XML response document.</param>
    /// <returns>The namespace URI string, or an empty string if no namespace is declared.</returns>
    private static string GetNamespace(XDocument doc)
    {
        return doc.Root?.Name.NamespaceName ?? string.Empty;
    }

    /// <summary>
    /// Builds a comma-separated nameserver string from the provided primary and optional secondary values.
    /// </summary>
    /// <param name="ns1">Primary nameserver hostname.</param>
    /// <param name="ns2">Optional secondary nameserver hostname.</param>
    /// <returns>Comma-separated nameserver list suitable for the Namecheap API.</returns>
    private static string BuildNameserverString(string ns1, string? ns2)
    {
        return ns2 is not null ? $"{ns1},{ns2}" : ns1;
    }
}
