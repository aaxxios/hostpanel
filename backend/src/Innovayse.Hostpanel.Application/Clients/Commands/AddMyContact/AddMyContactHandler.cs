using Innovayse.Hostpanel.Application.Clients.Commands.AddContact;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyProfile;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Commands.AddMyContact;

/// <summary>Resolves the caller's own client and adds the contact through the shared write.</summary>
public sealed class AddMyContactHandler
{
    /// <summary>Wolverine bus used to reach the profile read and the shared write.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus used to reach the profile read and the shared write.</param>
    public AddMyContactHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Handles <see cref="AddMyContactCommand"/>.</summary>
    /// <param name="cmd">The command. It names no account: this adds to the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record, or any
    /// refusal the shared write answers with.
    /// </returns>
    public async Task<Result> HandleAsync(AddMyContactCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var profile = await _bus.InvokeAsync<Result<ClientDto>>(new GetMyProfileQuery(), ct);
        if (profile.IsFailure)
        {
            return Result.FailureCode(profile.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result>(
            new AddContactCommand(
                profile.Value!.Id,
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
