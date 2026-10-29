using Innovayse.Hostpanel.Application.Domains.Common;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>A registrar whose renewal call throws, as a timeout does: the outcome is unknown.</summary>
public sealed class ThrowingRegistrar : TestRegistrar
{
    /// <inheritdoc/>
    protected override Task<RegistrarResult> Renew(DomainRenewalOrder request, StubRegistrarProvider stub, CancellationToken ct)
        => throw new HttpRequestException("registrar timed out");
}
