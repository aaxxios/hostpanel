using FluentValidation;
using Wolverine;

namespace Innovayse.Hostpanel.Shared.Middleware;

/// <summary>
/// Wolverine middleware that runs FluentValidation before each message handler.
/// Throws <see cref="ValidationException"/> if any validators report errors.
/// </summary>
/// <remarks>
/// <see cref="Infrastructure.ValidationPolicy"/> closes this type over each chain's message type, so
/// the generated handler code receives the message's own validators through the constructor and no
/// validator is looked up from the service provider at run time.
/// </remarks>
/// <typeparam name="TMessage">The message type of the chain this middleware guards.</typeparam>
public sealed class ValidationMiddleware<TMessage>
    where TMessage : class
{
    /// <summary>Every validator registered for <typeparamref name="TMessage"/>; empty when it has none.</summary>
    private readonly IReadOnlyList<IValidator<TMessage>> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationMiddleware{TMessage}"/> class.
    /// </summary>
    /// <param name="validators">Every validator registered for <typeparamref name="TMessage"/>; empty when it has none.</param>
    public ValidationMiddleware(IEnumerable<IValidator<TMessage>> validators)
    {
        _validators = validators.ToList();
    }

    /// <summary>
    /// Runs before the message handler: executes every validator of the message. Throws
    /// <see cref="ValidationException"/> if any rule fails; otherwise returns
    /// <see cref="HandlerContinuation.Continue"/>.
    /// </summary>
    /// <param name="message">The incoming message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="HandlerContinuation.Continue"/> when the message passed.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validation rules fail.</exception>
    public async Task<HandlerContinuation> BeforeAsync(TMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_validators.Count == 0)
        {
            return HandlerContinuation.Continue;
        }

        var context = new ValidationContext<TMessage>(message);
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return HandlerContinuation.Continue;
    }
}
