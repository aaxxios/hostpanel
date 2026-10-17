using Innovayse.Hostpanel.Application.Auth.Commands.SendVerificationEmail;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Auth.Commands.ResendVerification;

/// <summary>
/// Handles <see cref="ResendVerificationCommand"/>: sends the caller the verification mail again,
/// unless the address is already confirmed, in which case it does nothing.
/// </summary>
public sealed class ResendVerificationHandler
{
    /// <summary>Who is calling; the account to verify is always the caller's own.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Wolverine message bus, which carries the actual send.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="caller">Who is calling.</param>
    /// <param name="bus">Wolverine message bus, which carries the actual send.</param>
    public ResendVerificationHandler(ICurrentUserService caller, IMessageBus bus)
    {
        _caller = caller;
        _bus = bus;
    }

    /// <summary>Sends the mail again, or does nothing when the address is already confirmed.</summary>
    /// <param name="cmd">The command; it names no account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success (including the already-confirmed no-op), <c>UNAUTHORIZED</c> when the credential names
    /// no subject, or the refusal <see cref="SendVerificationEmailHandler"/> gives.
    /// </returns>
    public async Task<Result> HandleAsync(ResendVerificationCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var subject = _caller.UserId;
        if (subject is null)
        {
            return Result.FailureCode(SharedErrorCodes.Unauthorized);
        }

        // The already-verified no-op lives in the send handler, so registration and resend share it.
        return await _bus.InvokeAsync<Result>(new SendVerificationEmailCommand(subject), ct);
    }
}
