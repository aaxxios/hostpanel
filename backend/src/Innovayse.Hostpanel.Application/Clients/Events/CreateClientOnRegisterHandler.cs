using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.IntegrationEvents.Contracts;
using Innovayse.Hostpanel.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Events;

/// <summary>
/// Wolverine handler that reacts to <see cref="ClientRegisteredIntegrationEvent"/>. Automatically
/// creates a <see cref="Client"/> record for every new registered user. This handler is
/// idempotent: if a client record already exists for the user, it does nothing.
/// </summary>
public sealed class CreateClientOnRegisterHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">Reads the current moment.</param>
    public CreateClientOnRegisterHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>Creates a Client record when a new user registers.</summary>
    /// <param name="evt">The integration event published by <c>RegisterHandler</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ClientRegisteredIntegrationEvent evt, CancellationToken ct)
    {
        var existing = await _context.Clients.AsNoTracking().AnyAsync(c => c.UserId == evt.UserId, ct);
        if (existing)
        {
            return;
        }

        var client = Client.Create(
            evt.UserId, evt.FirstName, evt.LastName, evt.Email, _clock.UtcNow,
            registration: new RegistrationInfo(evt.IpAddress, evt.UserAgent, evt.DeviceType, evt.OperatingSystem, evt.Browser));
        _context.Clients.Add(client);
        await _context.SaveChangesAsync(ct);
    }
}
