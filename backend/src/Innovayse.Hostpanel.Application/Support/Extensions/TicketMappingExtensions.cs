using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Domain.Support;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Extensions;

/// <summary>
/// The pieces the staff ticket read and the client's own ticket read share: the reply mapping and
/// the department name lookup, so neither copy can drift from the other.
/// </summary>
public static class TicketMappingExtensions
{
    /// <summary>Maps one ticket reply to <see cref="TicketReplyDto"/>.</summary>
    /// <param name="reply">The ticket reply entity.</param>
    /// <returns>The mapped DTO.</returns>
    public static TicketReplyDto ToDto(this TicketReply reply)
    {
        ArgumentNullException.ThrowIfNull(reply);

        return new TicketReplyDto(reply.Id, reply.Message, reply.AuthorName, reply.IsStaffReply, reply.CreatedAt);
    }

    /// <summary>Reads the name of the department a ticket is filed under.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="departmentId">The department id; null when the ticket has none.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The department name, or null when there is no department or it no longer exists.</returns>
    public static async Task<string?> FindDepartmentNameAsync(
        this IHostpanelDbContext context, int? departmentId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!departmentId.HasValue)
        {
            return null;
        }

        return await context.Departments
            .AsNoTracking()
            .Where(d => d.Id == departmentId.Value)
            .Select(d => d.Name)
            .FirstOrDefaultAsync(ct);
    }
}
