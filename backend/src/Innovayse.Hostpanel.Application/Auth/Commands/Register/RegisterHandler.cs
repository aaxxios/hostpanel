using Innovayse.Hostpanel.Application.Auth.Commands.SendVerificationEmail;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.IntegrationEvents.Contracts;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Auth.Commands.Register;

/// <summary>
/// Handles <see cref="RegisterCommand"/>: creates the account, gives it the client role, announces it
/// so the client record is created, and mails the verification link.
/// </summary>
/// <remarks>
/// It depends on a service that is registered only where this deployment owns its people. That is
/// safe because the route answers 404 before the bus is reached in any other mode, so this handler
/// is never resolved there.
/// </remarks>
public sealed class RegisterHandler
{
    /// <summary>The local user service, which creates the account.</summary>
    private readonly IUserService _users;

    /// <summary>Wolverine message bus, for the integration event and the verification mail.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Where refusals and a mail that could not be sent are logged.</summary>
    private readonly ILogger<RegisterHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="users">The local user service, which creates the account.</param>
    /// <param name="bus">Wolverine message bus, for the integration event and the verification mail.</param>
    /// <param name="logger">Where refusals and a mail that could not be sent are logged.</param>
    public RegisterHandler(IUserService users, IMessageBus bus, ILogger<RegisterHandler> logger)
    {
        _users = users;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Creates the account and everything an account needs to be usable.</summary>
    /// <param name="cmd">Email, password and name of the new account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The new account's id, or <c>REGISTRATION_FAILED</c> when the account cannot be created.</returns>
    public async Task<Result<RegisteredUserDto>> HandleAsync(RegisterCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        string userId;
        try
        {
            userId = await _users.CreateAsync(
                new CreateUserInput(cmd.Email, cmd.Password, cmd.FirstName, cmd.LastName), ct);

            // A user row on its own is not an account anyone can use: without the role every client
            // endpoint answers 403, and without the event there is no client record behind it.
            await _users.AddToRoleAsync(userId, Roles.Client, ct);
            await _bus.PublishAsync(new ClientRegisteredIntegrationEvent(
                userId, cmd.Email, cmd.FirstName, cmd.LastName));
        }
        catch (Exception ex)
        {
            // The identity layer's message is an English, internal sentence; it goes to the log
            // and the caller gets the localized one for the code.
            _logger.LogWarning(ex, "Local registration was refused.");
            return Result<RegisteredUserDto>.FailureCode(HostpanelErrorCodes.REGISTRATION_FAILED);
        }

        // Outside the try on purpose: the account exists by now, so a mail that cannot be sent
        // must not turn a successful registration into REGISTRATION_FAILED. The user can ask for
        // the link again through POST /api/auth/resend-verification.
        var verification = await _bus.InvokeAsync<Result>(new SendVerificationEmailCommand(userId), ct);
        if (verification.IsFailure)
        {
            _logger.LogWarning(
                "The verification email for a new account was not sent: {ErrorCode}.", verification.ErrorCode);
        }

        return Result<RegisteredUserDto>.Success(new RegisteredUserDto(userId));
    }
}
