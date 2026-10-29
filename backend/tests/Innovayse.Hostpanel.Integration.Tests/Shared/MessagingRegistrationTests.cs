using Innovayse.Hostpanel.API.Extensions;
using Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment;
using Innovayse.Hostpanel.Application.Billing.Commands.ReconcileGatewayPaymentsCron;
using Innovayse.Hostpanel.Shared.Contracts.Interfaces;
using Innovayse.Hostpanel.Shared.Settings;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using Wolverine.Runtime;
using Wolverine.Runtime.Handlers;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>
/// <see cref="MessagingRegistration"/>: how Wolverine gets its handler code in each environment, the
/// execution timeout the chains of the recurring jobs get in place of the one-minute default, and the
/// refusal of service location that every chain is built under.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class MessagingRegistrationTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A deployed host loads handler code generated at build time and refuses a handler it has none for;
    /// Development compiles on demand when nothing was generated, and the test host always compiles in
    /// memory, so the code of its test doubles is never written into the API's source folder.
    /// </summary>
    /// <param name="environment">The environment name.</param>
    /// <param name="expected">The mode that environment gets.</param>
    [Theory]
    [InlineData("Production", TypeLoadMode.Static)]
    [InlineData("Staging", TypeLoadMode.Static)]
    [InlineData("Demo", TypeLoadMode.Static)]
    [InlineData("Development", TypeLoadMode.Auto)]
    [InlineData("Testing", TypeLoadMode.Dynamic)]
    public void TypeLoadMode_FollowsTheEnvironment(string environment, TypeLoadMode expected)
    {
        MessagingRegistration.TypeLoadModeFor(new HostingEnvironment { EnvironmentName = environment }).Should().Be(expected);
    }

    /// <summary>
    /// Every recurring job command has a chain that may run as long as the job lease, so a pass over
    /// many items is not cut off at Wolverine's default of one minute.
    /// </summary>
    [Fact]
    public void RecurringJobChains_MayRunAsLongAsTheJobLease()
    {
        var lease = factory.Services.GetRequiredService<IOptions<ScheduledJobsOptions>>().Value.Lease;
        var jobs = typeof(ReconcileGatewayPaymentsCronCommand).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IRecurringJob).IsAssignableFrom(t))
            .ToList();
        var chains = factory.Services.GetRequiredService<HandlerGraph>().Chains;

        jobs.Should().HaveCountGreaterThanOrEqualTo(13);
        foreach (var job in jobs)
        {
            chains.Single(c => c.MessageType == job).ExecutionTimeoutInSeconds
                .Should().Be((int)lease.TotalSeconds, job.Name);
        }
    }

    /// <summary>A chain that is not a recurring job keeps the default timeout.</summary>
    [Fact]
    public void OtherChains_KeepTheDefaultTimeout()
    {
        var lease = factory.Services.GetRequiredService<IOptions<ScheduledJobsOptions>>().Value.Lease;
        var chains = factory.Services.GetRequiredService<HandlerGraph>().Chains;

        chains.Single(c => c.MessageType == typeof(CompleteGatewayPaymentCommand)).ExecutionTimeoutInSeconds
            .Should().NotBe((int)lease.TotalSeconds);
    }

    /// <summary>
    /// The host refuses to fall back on the container in generated handler code; relaxing it would hide
    /// every future lambda registration, <see cref="IServiceProvider"/> dependency or non-public adapter.
    /// </summary>
    [Fact]
    public void ServiceLocation_IsNotAllowed()
    {
        var runtime = factory.Services.GetRequiredService<IWolverineRuntime>();

        runtime.Options.ServiceLocationPolicy.Should().Be(ServiceLocationPolicy.NotAllowed);
    }

    /// <summary>
    /// Every handler chain of the API builds from constructors alone. Wolverine builds a chain at the
    /// first message of its type, so without this a chain that needs service location would fail only
    /// when that message arrives: a sign-in, or a recurring job that then never runs.
    /// </summary>
    [Fact]
    public void EveryHandlerChain_BuildsWithoutServiceLocation()
    {
        var offenders = ChainsThatFailToBuild(factory);

        offenders.Should().BeEmpty("every chain must be built from constructors; offenders: {0}", string.Join("; ", offenders));
    }

    /// <summary>
    /// The same holds with the public demo switched on, whose container swaps every outward port for an
    /// offline fake, so those fakes are held to the rule as well.
    /// </summary>
    [Fact]
    public void EveryHandlerChain_OfTheDemo_BuildsWithoutServiceLocation()
    {
        var offenders = ChainsThatFailToBuild(DemoTestHost.Enabled(factory));

        offenders.Should().BeEmpty("every chain must be built from constructors; offenders: {0}", string.Join("; ", offenders));
    }

    /// <summary>Builds the handler of every chain the host discovered and names the ones that fail.</summary>
    /// <param name="host">The host whose chains are built.</param>
    /// <returns>One line per chain that could not be built: its message type and the first line of the reason.</returns>
    private static List<string> ChainsThatFailToBuild(WebApplicationFactory<Program> host)
    {
        var graph = host.Services.GetRequiredService<HandlerGraph>();
        var offenders = new List<string>();

        graph.Chains.Should().NotBeEmpty("the handlers of the API and Application assemblies are discovered");
        foreach (var chain in graph.Chains)
        {
            try
            {
                graph.HandlerFor(chain.MessageType);
            }
            catch (Exception exception)
            {
                var reason = exception.Message
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .FirstOrDefault() ?? exception.GetType().Name;
                offenders.Add($"{chain.MessageType.Name}: {exception.GetType().Name}: {reason}");
            }
        }

        return offenders;
    }
}
