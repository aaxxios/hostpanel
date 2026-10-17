using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Shared.Middleware;

/// <summary>
/// Wolverine middleware that runs FluentValidation before each message handler.
/// Throws <see cref="ValidationException"/> if any validators report errors.
/// Uses <see cref="Envelope"/> to remain message-type-agnostic (Wolverine does not
/// support generic Before/After methods — <c>T</c> would be treated as a DI variable).
/// </summary>
public class ValidationMiddleware
{
    /// <summary>Used to resolve <see cref="IValidator{T}"/> instances at runtime.</summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="ValidationMiddleware"/>.
    /// </summary>
    /// <param name="serviceProvider">Used to resolve <see cref="IValidator{T}"/> instances at runtime.</param>
    public ValidationMiddleware(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Runs before the message handler. Resolves all registered <see cref="IValidator{T}"/>
    /// for the message type and executes them. Throws <see cref="ValidationException"/>
    /// if any validation rules fail; otherwise returns <see cref="HandlerContinuation.Continue"/>.
    /// </summary>
    /// <param name="envelope">Wolverine envelope containing the incoming message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="HandlerContinuation.Continue"/> when the message passed.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validation rules fail.</exception>
    public async Task<HandlerContinuation> BeforeAsync(Envelope envelope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var message = envelope.Message;
        if (message == null)
        {
            return HandlerContinuation.Continue;
        }

        var messageType = message.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(messageType);
        var validators = _serviceProvider.GetServices(validatorType).ToList();

        if (validators.Count == 0)
        {
            return HandlerContinuation.Continue;
        }

        // Invoke typed validation via reflection to preserve IValidator<T> generics
        var validateMethod = typeof(ValidationMiddleware)
            .GetMethod(nameof(ValidateAsync), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(messageType);

        await (Task)validateMethod.Invoke(null, [message, validators, cancellationToken])!;

        return HandlerContinuation.Continue;
    }

    /// <summary>
    /// Executes all registered <see cref="IValidator{T}"/> for a strongly-typed message in parallel.
    /// Called via reflection from <see cref="BeforeAsync"/> to preserve generic type information.
    /// </summary>
    /// <typeparam name="T">The concrete message type.</typeparam>
    /// <param name="message">The message instance to validate.</param>
    /// <param name="validators">
    /// Untyped validator instances resolved from DI, cast to <see cref="IValidator{T}"/>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when every validator has run.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validation rules fail.</exception>
    private static async Task ValidateAsync<T>(T message, IList<object> validators, CancellationToken cancellationToken)
        where T : class
    {
        var typedValidators = validators.Cast<IValidator<T>>().ToList();
        var context = new ValidationContext<T>(message);

        var results = await Task.WhenAll(
            typedValidators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }
    }
}
