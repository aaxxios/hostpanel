using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetDomain;

/// <summary>Returns a full <see cref="DomainDto"/> including nameservers and DNS records.</summary>
public sealed class GetDomainHandler
{
    /// <summary>The data context the aggregate is read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the aggregate is read from.</param>
    public GetDomainHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles <see cref="GetDomainQuery"/>.
    /// </summary>
    /// <param name="query">The get domain query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The domain DTO with nameservers and DNS records, or <c>DOMAIN_NOT_FOUND</c> when the id
    /// names no domain.
    /// </returns>
    public async Task<Result<DomainDto>> HandleAsync(GetDomainQuery query, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == query.DomainId, ct);
        if (domain is null)
        {
            return Result.Failure<DomainDto>(
                $"Domain {query.DomainId} not found.", HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        var dto = domain.ToDto();

        return Result.Success(dto);
    }
}
