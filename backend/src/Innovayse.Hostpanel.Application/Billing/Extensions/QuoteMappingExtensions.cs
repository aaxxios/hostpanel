using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Application.Billing.Extensions;

/// <summary>
/// Maps <see cref="Quote"/> to <see cref="QuoteListItemDto"/>. Every query that returns this DTO projects
/// through this one mapping, so a field added to the DTO is added once rather than in several
/// handlers that quietly drift apart.
/// </summary>
public static class QuoteMappingExtensions
{
    /// <summary>Projects the query to <see cref="QuoteListItemDto"/> rows, translated into the SQL select list.</summary>
    /// <param name="source">The query to project.</param>
    /// <returns>The query projected to <see cref="QuoteListItemDto"/>.</returns>
    public static IQueryable<QuoteListItemDto> SelectDto(this IQueryable<Quote> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(q => new QuoteListItemDto(
            q.Id, q.ClientId, string.Empty, q.Subject, q.CreatedAt, q.ExpiryDate, q.Total, q.Stage));
    }
}
