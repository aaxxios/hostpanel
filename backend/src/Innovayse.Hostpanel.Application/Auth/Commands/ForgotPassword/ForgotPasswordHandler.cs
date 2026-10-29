using Innovayse.Hostpanel.Application.Auth.Commands.RequestPasswordReset;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Handles <see cref="ForgotPasswordCommand"/>: hands the request to the background and answers at
/// once, so the response is the same, in the same time, whether or not the address belongs to an
/// account.
/// </summary>
public sealed class ForgotPasswordHandler
{
    /// <summary>Wolverine message bus, which carries the reset to the background.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine message bus, which carries the reset to the background.</param>
    public ForgotPasswordHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Publishes the reset request and answers.</summary>
    /// <param name="cmd">The address the reset link is asked for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, whether or not the address belongs to an account.</returns>
    public async Task<Result> HandleAsync(ForgotPasswordCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // Published, not invoked: an existing account costs a token, a template seed and a mail,
        // and waiting for them here would tell the caller by the response time which addresses exist.
        await _bus.PublishAsync(new RequestPasswordResetCommand(cmd.Email));
        return Result.Success();
    }
}
