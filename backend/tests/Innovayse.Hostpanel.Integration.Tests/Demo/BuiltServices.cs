using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Demo;

/// <summary>
/// A service collection and the provider built from it.
/// </summary>
/// <param name="Services">The registrations.</param>
/// <param name="Provider">The built provider.</param>
public sealed record BuiltServices(ServiceCollection Services, ServiceProvider Provider);
