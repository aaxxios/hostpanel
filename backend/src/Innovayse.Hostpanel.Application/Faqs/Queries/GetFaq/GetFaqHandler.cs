using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Faqs.Common;
using Innovayse.Hostpanel.Application.Faqs.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Faqs.Queries.GetFaq;

/// <summary>Loads a single question with every translation and maps it to <see cref="FaqAdminDto"/>.</summary>
public sealed class GetFaqHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public GetFaqHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The question id to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The admin DTO, or <c>FAQ_NOT_FOUND</c> when the id names no question.</returns>
    public async Task<Result<FaqAdminDto>> HandleAsync(GetFaqQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var entity = await _context.Faqs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (entity is null)
        {
            return Result<FaqAdminDto>.FailureCode(HostpanelErrorCodes.FAQ_NOT_FOUND);
        }

        return Result<FaqAdminDto>.Success(entity.ToAdminDto());
    }
}
