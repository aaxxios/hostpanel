using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetClient;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Domains.Registrar;

/// <summary>
/// Builds the registrant contact a registrar order carries from the client's own details, so a
/// domain is registered to the person who bought it and never to a blank contact.
/// </summary>
public sealed class RegistrantContactReader
{
    /// <summary>The Wolverine bus the Clients module's client query is dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes a new instance of the <see cref="RegistrantContactReader"/> class.</summary>
    /// <param name="bus">The Wolverine bus the Clients module's client query is dispatched through.</param>
    public RegistrantContactReader(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// Reads the client's details as a registrant contact. A detail the client never filled in
    /// comes back empty; whether the contact is complete is decided where it is sent
    /// (<see cref="Extensions.DomainContactExtensions.IsCompleteForPurchase"/>).
    /// </summary>
    /// <param name="clientId">The client the domain is bought for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The contact, or <c>CLIENT_NOT_FOUND</c> when the id names no client.</returns>
    public async Task<Result<DomainContact>> ReadAsync(int clientId, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ClientDto>>(new GetClientQuery(clientId), ct);
        if (outcome.IsFailure)
        {
            return Result<DomainContact>.FailureCode(outcome.ErrorCode!);
        }

        var client = outcome.Value!;

        return Result.Success(new DomainContact(
            client.FirstName,
            client.LastName,
            client.CompanyName,
            client.Email,
            client.Phone ?? string.Empty,
            client.Address.Street ?? string.Empty,
            client.Address.Address2,
            client.Address.City ?? string.Empty,
            client.Address.State ?? string.Empty,
            client.Address.PostCode ?? string.Empty,
            client.Address.Country ?? string.Empty));
    }
}
