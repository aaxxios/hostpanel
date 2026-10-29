using System.Net;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>
/// The rate limiter binds the <c>RateLimit</c> section from the host's configuration service,
/// refuses an invalid budget, and derives the forwarded-header limit from the same section. Built on
/// a bare service collection, so no database is involved.
/// </summary>
public sealed class RateLimitingRegistrationTests
{
    /// <summary>Registers the limiter on a bare collection over the given settings.</summary>
    /// <param name="settings">The configuration values.</param>
    /// <returns>The provider built from the collection.</returns>
    private static ServiceProvider Build(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPlatformRateLimiting(new TestHostEnvironment());
        return services.BuildServiceProvider();
    }

    /// <summary>Values in the section reach the bound options.</summary>
    [Fact]
    public void Section_IsBound()
    {
        using var provider = Build(new()
        {
            ["RateLimit:Global:PermitLimit"] = "500",
            ["RateLimit:TrustedProxyCount"] = "2",
        });

        var limits = provider.GetRequiredService<IOptions<RateLimitOptions>>().Value;

        limits.Global.PermitLimit.Should().Be(500);
        limits.TrustedProxyCount.Should().Be(2);
    }

    /// <summary>The forwarded-header limit is the section's trusted proxy count.</summary>
    [Fact]
    public void ForwardLimit_ComesFromTrustedProxyCount()
    {
        using var provider = Build(new() { ["RateLimit:TrustedProxyCount"] = "3" });

        var forwarded = provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;

        forwarded.ForwardLimit.Should().Be(3);
        forwarded.KnownProxies.Should().BeEmpty();
        forwarded.KnownNetworks.Should().BeEmpty();
    }

    /// <summary>With nothing configured no proxy is trusted: the count is zero and no proxy or network is known.</summary>
    [Fact]
    public void Default_TrustsNoProxy()
    {
        using var provider = Build([]);

        var forwarded = provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;

        forwarded.ForwardLimit.Should().Be(0);
        forwarded.KnownProxies.Should().BeEmpty();
        forwarded.KnownNetworks.Should().BeEmpty();
    }

    /// <summary>The configured proxies and networks are the ones the forwarded headers are believed from.</summary>
    [Fact]
    public void KnownProxiesAndNetworks_ComeFromTheSection()
    {
        using var provider = Build(new()
        {
            ["RateLimit:TrustedProxyCount"] = "1",
            ["RateLimit:KnownProxies:0"] = "10.0.0.5",
            ["RateLimit:KnownNetworks:0"] = "172.18.0.0/16",
        });

        var forwarded = provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;

        forwarded.KnownProxies.Should().ContainSingle().Which.Should().Be(IPAddress.Parse("10.0.0.5"));
        var network = forwarded.KnownNetworks.Should().ContainSingle().Subject;
        network.Prefix.Should().Be(IPAddress.Parse("172.18.0.0"));
        network.PrefixLength.Should().Be(16);
    }

    /// <summary>A proxy or network entry that does not parse refuses startup.</summary>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The malformed value.</param>
    [Theory]
    [InlineData("RateLimit:KnownProxies:0", "not-an-address")]
    [InlineData("RateLimit:KnownNetworks:0", "172.18.0.0/99")]
    public void MalformedProxyEntry_IsRefused(string key, string value)
    {
        using var provider = Build(new() { [key] = value });

        var read = () => provider.GetRequiredService<IOptions<RateLimitOptions>>().Value;

        read.Should().Throw<OptionsValidationException>();
    }

    /// <summary>A tier larger than the global budget is refused.</summary>
    [Fact]
    public void TierAboveGlobal_IsRefused()
    {
        using var provider = Build(new()
        {
            ["RateLimit:Global:PermitLimit"] = "10",
            ["RateLimit:Auth:PermitLimit"] = "20",
        });

        var read = () => provider.GetRequiredService<IOptions<RateLimitOptions>>().Value;

        read.Should().Throw<OptionsValidationException>();
    }

    /// <summary>A non-positive window is refused.</summary>
    [Fact]
    public void NonPositiveWindow_IsRefused()
    {
        using var provider = Build(new() { ["RateLimit:Strict:WindowSeconds"] = "0" });

        var read = () => provider.GetRequiredService<IOptions<RateLimitOptions>>().Value;

        read.Should().Throw<OptionsValidationException>();
    }

    /// <summary>A Development host environment, so the limiter is switched on.</summary>
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        /// <inheritdoc/>
        public string EnvironmentName { get; set; } = Environments.Development;

        /// <inheritdoc/>
        public string ApplicationName { get; set; } = nameof(RateLimitingRegistrationTests);

        /// <inheritdoc/>
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        /// <inheritdoc/>
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
