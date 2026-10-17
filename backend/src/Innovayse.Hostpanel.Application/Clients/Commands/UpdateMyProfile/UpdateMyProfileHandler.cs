using Innovayse.Hostpanel.Application.Clients.Commands.UpdateClient;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyProfile;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateMyProfile;

/// <summary>
/// Resolves the caller's own client, applies the update through the shared write, and answers with
/// the profile as it now stands.
/// </summary>
public sealed class UpdateMyProfileHandler
{
    /// <summary>Wolverine bus used to reach the profile read and the shared write.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus used to reach the profile read and the shared write.</param>
    public UpdateMyProfileHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Handles <see cref="UpdateMyProfileCommand"/>.</summary>
    /// <param name="cmd">The command. It names no account: this updates the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The updated profile, <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record,
    /// or any refusal the shared write answers with.
    /// </returns>
    public async Task<Result<ClientDto>> HandleAsync(UpdateMyProfileCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var profile = await _bus.InvokeAsync<Result<ClientDto>>(new GetMyProfileQuery(), ct);
        if (profile.IsFailure)
        {
            return profile;
        }

        // Currency is deliberately null: it is set once, at the first order or by the admin who
        // created the client, and only an admin may change it before the first invoice.
        var update = new UpdateClientCommand(
            profile.Value!.Id,
            cmd.Email,
            cmd.FirstName,
            cmd.LastName,
            cmd.CompanyName,
            cmd.Phone,
            cmd.Street,
            cmd.Address2,
            cmd.City,
            cmd.State,
            cmd.PostCode,
            cmd.Country,
            cmd.Language,
            null,
            cmd.PaymentMethod,
            cmd.BillingContact,
            cmd.AdminNotes,
            cmd.NotifyGeneral,
            cmd.NotifyInvoice,
            cmd.NotifySupport,
            cmd.NotifyProduct,
            cmd.NotifyDomain,
            cmd.NotifyAffiliate,
            cmd.LateFees,
            cmd.OverdueNotices,
            cmd.TaxExempt,
            cmd.SeparateInvoices,
            cmd.DisableCcProcessing,
            cmd.MarketingOptIn,
            cmd.StatusUpdate,
            cmd.AllowSso,
            cmd.Status);

        var written = await _bus.InvokeAsync<Result>(update, ct);
        if (written.IsFailure)
        {
            return Result<ClientDto>.FailureCode(written.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result<ClientDto>>(new GetMyProfileQuery(), ct);
    }
}
