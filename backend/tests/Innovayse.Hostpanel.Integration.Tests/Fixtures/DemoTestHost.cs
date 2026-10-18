using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Builds hosts with the public demo switched on, over the same PostgreSQL container as the shared
/// factory. The switch is a host setting (<c>Demo__*</c> in production) because the composition
/// root reads it while the container is still being built.
/// </summary>
public static class DemoTestHost
{
    /// <summary>The published demo administrator's email.</summary>
    public const string AdminEmail = "demo-admin@demo.example";

    /// <summary>The published demo administrator's password.</summary>
    public const string AdminPassword = "Demo-Admin-Pass-1";

    /// <summary>The published demo client's email.</summary>
    public const string ClientEmail = "demo-client@demo.example";

    /// <summary>The published demo client's password.</summary>
    public const string ClientPassword = "Demo-Client-Pass-1";

    /// <summary>Hours between resets on the demo test hosts.</summary>
    public const int ResetIntervalHours = 24;

    /// <summary>Derives a host with the demo enabled and every required value set.</summary>
    /// <param name="factory">The shared API + PostgreSQL host to derive from.</param>
    /// <returns>A host whose container, adapters and guard are the demo's.</returns>
    public static WebApplicationFactory<Program> Enabled(IntegrationTestFactory factory) =>
        factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Demo:Enabled", "true");
            builder.UseSetting("Demo:ResetIntervalHours", ResetIntervalHours.ToString(System.Globalization.CultureInfo.InvariantCulture));
            builder.UseSetting("Demo:AdminEmail", AdminEmail);
            builder.UseSetting("Demo:AdminPassword", AdminPassword);
            builder.UseSetting("Demo:ClientEmail", ClientEmail);
            builder.UseSetting("Demo:ClientPassword", ClientPassword);
        });

    /// <summary>
    /// Derives a host that is composed as a demo (the container reads the switch while it is being
    /// built) but whose options then report the demo as off, so the guards inside the real services
    /// can be exercised: the manager and the reset handler must do nothing outside a demo.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host to derive from.</param>
    /// <returns>A host whose services are the demo's and whose <see cref="DemoOptions.Enabled"/> is false.</returns>
    public static WebApplicationFactory<Program> EnabledThenSwitchedOff(IntegrationTestFactory factory) =>
        Enabled(factory).WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton(Options.Create(new DemoOptions
            {
                Enabled = false,
                ResetIntervalHours = ResetIntervalHours,
                AdminEmail = AdminEmail,
                AdminPassword = AdminPassword,
                ClientEmail = ClientEmail,
                ClientPassword = ClientPassword,
            }))));
}
