using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Clients.Commands.CreateClient;

/// <summary>Creates and persists a new <see cref="Client"/> aggregate.</summary>
public sealed class CreateClientHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateClientHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Creates a new client record and saves it.</summary>
    /// <param name="cmd">The create client command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The id of the newly created client.</returns>
    public async Task<Result<int>> HandleAsync(CreateClientCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var client = Client.Create(cmd.SubjectUserId, cmd.FirstName, cmd.LastName, cmd.Email, cmd.CompanyName);
        _context.Clients.Add(client);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(client.Id);
    }
}
