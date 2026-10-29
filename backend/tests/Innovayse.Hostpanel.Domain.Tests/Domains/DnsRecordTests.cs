using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Domains;

/// <summary>Unit tests for <see cref="DnsRecord"/>.</summary>
public sealed class DnsRecordTests
{
    /// <summary>CreateDetached builds a record carrying every field and bound to no domain.</summary>
    [Fact]
    public void CreateDetached_CarriesFieldsWithoutADomain()
    {
        var record = DnsRecord.CreateDetached(DnsRecordType.A, "@", "1.2.3.4", 3600, 10);

        record.DomainId.Should().Be(0);
        record.Type.Should().Be(DnsRecordType.A);
        record.Host.Should().Be("@");
        record.Value.Should().Be("1.2.3.4");
        record.Ttl.Should().Be(3600);
        record.Priority.Should().Be(10);
    }
}
