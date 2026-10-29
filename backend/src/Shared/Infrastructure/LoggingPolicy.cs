using Innovayse.Hostpanel.Shared.Middleware;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using Wolverine.Configuration;
using Wolverine.Runtime.Handlers;

namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// Adds <see cref="LoggingMiddleware"/> to every handler chain: its <c>BeforeAsync</c> after the
/// middleware already in the chain (so validation, inserted at position 0, runs first and a refused
/// message is not logged as handled) and its <c>After</c> as a postprocessor.
/// </summary>
public sealed class LoggingPolicy : IHandlerPolicy
{
    /// <inheritdoc />
    public void Apply(IReadOnlyList<HandlerChain> chains, GenerationRules rules, IServiceContainer container)
    {
        ArgumentNullException.ThrowIfNull(chains);

        foreach (var chain in chains)
        {
            var before = new MethodCall(typeof(LoggingMiddleware), nameof(LoggingMiddleware.BeforeAsync));

            // ValidationMiddleware<TMessage>.BeforeAsync also returns a HandlerContinuation, and codegen names
            // each result after its method: two "result_of_BeforeAsync" locals fail to compile (CS0128).
            foreach (var created in before.Creates)
            {
                created.OverrideName("logging_continuation");
            }

            chain.Middleware.Add(before);
            chain.Postprocessors.Add(new MethodCall(typeof(LoggingMiddleware), nameof(LoggingMiddleware.After)));
        }
    }
}
