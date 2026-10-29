using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// The service registrations of a host exactly as the composition root left them, taken before anything
/// else is added. Registered only by <see cref="IntegrationTestFactory.WithProductionAdapters"/>, whose
/// hosts replace no adapter, so the list is the production container of the host's configuration.
/// </summary>
/// <param name="Descriptors">Every registration the composition root made, in registration order.</param>
public sealed record ServiceRegistrationSnapshot(IReadOnlyList<ServiceDescriptor> Descriptors);
