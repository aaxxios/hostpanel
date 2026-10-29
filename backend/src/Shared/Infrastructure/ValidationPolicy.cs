using Innovayse.Hostpanel.Shared.Middleware;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using Wolverine.Configuration;
using Wolverine.Runtime.Handlers;

namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// Inserts <see cref="ValidationMiddleware{TMessage}"/>, closed over the chain's message type, at the
/// front of every handler chain. A policy, never an opt-in marker: a handler whose author forgot the
/// marker would be silently unvalidated and look exactly like one whose rules passed.
/// </summary>
public sealed class ValidationPolicy : IHandlerPolicy
{
    /// <inheritdoc />
    public void Apply(IReadOnlyList<HandlerChain> chains, GenerationRules rules, IServiceContainer container)
    {
        foreach (var chain in chains)
        {
            var middleware = typeof(ValidationMiddleware<>).MakeGenericType(chain.MessageType);
            chain.Middleware.Insert(0, new MethodCall(middleware, nameof(ValidationMiddleware<object>.BeforeAsync)));
        }
    }
}
