using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>Boots Program.cs with the settings a provider-owned deployment supplies.</summary>
internal sealed class SsoModeFactory(string connectionString) : WebApplicationFactory<Program>
{
    /// <summary>Boots the host in the Testing environment, in single sign-on mode, against the test database.</summary>
    /// <param name="builder">The web host being configured.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting rather than ConfigureAppConfiguration, for the reason spelled out
        // in IntegrationTestFactory: Program.cs is top-level code and reads these while
        // composing the builder, before ConfigureAppConfiguration's sources exist.
        builder.UseSetting("Auth:Mode", "sso");
        // Base64 of 32 bytes, not 32 characters: AddInfrastructure decodes it and
        // refuses anything that is not exactly a 256-bit key. Same value the rest of
        // the suite uses.
        builder.UseSetting("EncryptionKey", "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=");

        // The sign-on package's registration validates that each of these is present and refuses to start
        // naming the one that is missing. None is contacted while the host is built:
        // the bearer handler fetches discovery lazily, on the first token it is asked
        // to validate, and no test here presents one.
        builder.UseSetting("Auth:AppName", "hostpanel");
        builder.UseSetting("Auth:Authority", "https://sso.invalid");
        builder.UseSetting("Auth:PublicAuthority", "https://sso.invalid");
        builder.UseSetting("Auth:ClientId", "hostpanel");
        builder.UseSetting("Auth:ClientSecret", "test-secret");
        builder.UseSetting("Auth:RedisConnection", "localhost:6379");
        builder.UseSetting("Sso:Authority", "https://sso.invalid");
        builder.UseSetting("Sso:ClientId", "hostpanel");
        builder.UseSetting("Sso:ServiceKey", "test-service-key");

        // Required whenever Auth:Mode is not local (BackchannelLogoutOptionsValidator), so an external sign-on provider host
        // refuses to boot without it.
        builder.UseSetting("BackchannelLogout:Key", "test-backchannel-key");

        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
            }));
    }
}
