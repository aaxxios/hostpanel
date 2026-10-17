using Innovayse.Hostpanel.Application.Auth.Commands.CompleteSetup;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Auth.Commands.ClaimSetup;

/// <summary>Runs the first-run bootstrap and answers with the acknowledgement the endpoint returns.</summary>
public sealed class ClaimSetupHandler
{
    /// <summary>Wolverine bus used to reach the shared write.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus used to reach the shared write.</param>
    public ClaimSetupHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Handles <see cref="ClaimSetupCommand"/>.</summary>
    /// <param name="cmd">The claim, carrying the setup token and nothing else.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The acknowledgement, or whichever refusal <see cref="CompleteSetupCommand"/> answers with.</returns>
    public async Task<Result<SetupClaimedDto>> HandleAsync(ClaimSetupCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var granted = await _bus.InvokeAsync<Result>(new CompleteSetupCommand(cmd.SetupToken), ct);
        if (granted.IsFailure)
        {
            return Result<SetupClaimedDto>.FailureCode(granted.ErrorCode!);
        }

        return Result<SetupClaimedDto>.Success(new SetupClaimedDto(true));
    }
}
