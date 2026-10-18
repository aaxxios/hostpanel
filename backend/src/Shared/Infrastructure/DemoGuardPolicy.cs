using Innovayse.Hostpanel.Shared.Contracts.Interfaces;
using Innovayse.Hostpanel.Shared.Middleware;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using Wolverine.Configuration;
using Wolverine.Runtime.Handlers;

namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// Puts <see cref="DemoGuardMiddleware"/> in front of every handler chain whose message implements
/// <see cref="IDemoRestricted"/>. A policy rather than a per-handler check: the one place that
/// knows demo mode exists is this class, and the marker on the command is the whole opt-in.
/// </summary>
public sealed class DemoGuardPolicy : IHandlerPolicy
{
    /// <inheritdoc />
    public void Apply(IReadOnlyList<HandlerChain> chains, GenerationRules rules, IServiceContainer container)
    {
        foreach (var chain in chains.Where(c => typeof(IDemoRestricted).IsAssignableFrom(c.MessageType)))
        {
            chain.Middleware.Insert(
                0, new MethodCall(typeof(DemoGuardMiddleware), nameof(DemoGuardMiddleware.Before)));
        }
    }
}
