using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="IProvisioningProviderFactory"/> that hands back the same
/// <see cref="RecordingProvisioningProvider"/> for every server, so a test can read the calls a
/// domain event handler made through it regardless of which server row triggered them.
/// </summary>
/// <param name="provider">The recording provider every server resolves to.</param>
public sealed class RecordingProvisioningProviderFactory(RecordingProvisioningProvider provider)
    : IProvisioningProviderFactory
{
    /// <inheritdoc/>
    public IProvisioningProvider CreateFor(ServerConnectionDto server) => provider;
}
