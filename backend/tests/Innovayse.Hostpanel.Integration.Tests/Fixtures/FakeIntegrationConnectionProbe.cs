using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="IIntegrationConnectionProbe"/> that counts how often it was asked and either accepts
/// the credentials or throws what a test told it to. A fake port standing in for a provider's live
/// login, which a test must not reach over the network.
/// </summary>
/// <param name="slug">The integration slug this probe answers for.</param>
public sealed class FakeIntegrationConnectionProbe(string slug) : IIntegrationConnectionProbe
{
    /// <summary>Gets how many times <see cref="ProbeAsync"/> was called.</summary>
    public int Calls { get; private set; }

    /// <summary>Gets or sets what the probe throws; <see langword="null"/> means the credentials are accepted.</summary>
    public Exception? Throws { get; set; }

    /// <inheritdoc/>
    public string Slug { get; } = slug;

    /// <inheritdoc/>
    public Task ProbeAsync(CancellationToken ct)
    {
        Calls++;
        return Throws is null ? Task.CompletedTask : Task.FromException(Throws);
    }
}
