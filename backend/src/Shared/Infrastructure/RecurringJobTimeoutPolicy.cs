using Innovayse.Hostpanel.Shared.Contracts.Interfaces;
using JasperFx;
using JasperFx.CodeGeneration;
using Wolverine.Configuration;
using Wolverine.Runtime.Handlers;

namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// Gives the handler chain of every <see cref="IRecurringJob"/> command its own execution timeout
/// instead of Wolverine's one-minute default. A recurring pass loops over every item its job owns
/// (every client with a renewal due, every registrar, every server), so it can run far longer than
/// a request handler; the default would cancel it part-way and leave the rest for the next day.
/// Every other chain keeps the default.
/// </summary>
public sealed class RecurringJobTimeoutPolicy : IHandlerPolicy
{
    /// <summary>The timeout every recurring job's chain gets, in whole seconds.</summary>
    private readonly int _timeoutSeconds;

    /// <summary>Initializes a new instance of the <see cref="RecurringJobTimeoutPolicy"/> class.</summary>
    /// <param name="timeout">How long a recurring pass may run before Wolverine cancels it; the job lease, since a pass that outlasts it is started a second time by another instance.</param>
    public RecurringJobTimeoutPolicy(TimeSpan timeout)
    {
        _timeoutSeconds = (int)Math.Ceiling(timeout.TotalSeconds);
    }

    /// <inheritdoc />
    public void Apply(IReadOnlyList<HandlerChain> chains, GenerationRules rules, IServiceContainer container)
    {
        ArgumentNullException.ThrowIfNull(chains);

        foreach (var chain in chains.Where(c => typeof(IRecurringJob).IsAssignableFrom(c.MessageType)))
        {
            chain.ExecutionTimeoutInSeconds = _timeoutSeconds;
        }
    }
}
