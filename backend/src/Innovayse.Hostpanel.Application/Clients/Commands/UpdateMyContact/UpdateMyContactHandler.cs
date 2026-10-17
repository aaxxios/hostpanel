using Innovayse.Hostpanel.Application.Clients.Commands.UpdateContact;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyProfile;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateMyContact;

/// <summary>Resolves the caller's own client and updates the contact through the shared write.</summary>
public sealed class UpdateMyContactHandler
{
    /// <summary>Wolverine bus used to reach the profile read and the shared write.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus used to reach the profile read and the shared write.</param>
    public UpdateMyContactHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Handles <see cref="UpdateMyContactCommand"/>.</summary>
    /// <param name="cmd">The command. It names no account: this updates the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record, or any
    /// refusal the shared write answers with, such as <c>MY_CONTACT_NOT_FOUND</c>.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateMyContactCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var profile = await _bus.InvokeAsync<Result<ClientDto>>(new GetMyProfileQuery(), ct);
        if (profile.IsFailure)
        {
            return Result.FailureCode(profile.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result>(
            new UpdateContactCommand(
                profile.Value!.Id,
                cmd.ContactId,
                cmd.FirstName,
                cmd.LastName,
                cmd.CompanyName,
                cmd.Email,
                cmd.Phone,
                cmd.Type,
                cmd.Street,
                cmd.Address2,
                cmd.City,
                cmd.State,
                cmd.PostCode,
                cmd.Country,
                cmd.NotifyGeneral,
                cmd.NotifyInvoice,
                cmd.NotifySupport,
                cmd.NotifyProduct,
                cmd.NotifyDomain,
                cmd.NotifyAffiliate),
            ct);
    }
}
