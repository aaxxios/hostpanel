using Innovayse.Hostpanel.Domain.Domains;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Extensions;

/// <summary>
/// Query fragments over <see cref="RegisteredDomain"/> that many use cases share. A domain is
/// loaded together with everything hanging off it far more often than on its own, so the include
/// list is written once here instead of at every call site.
/// </summary>
public static class DomainQueryExtensions
{
    /// <summary>
    /// Eagerly loads the nameservers, DNS records, email-forwarding rules and renewal reminders of
    /// every domain in the query.
    /// </summary>
    /// <param name="source">The domain query to load children for.</param>
    /// <returns>The query including all four child collections.</returns>
    public static IQueryable<RegisteredDomain> IncludeChildren(this IQueryable<RegisteredDomain> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .Include(x => x.Nameservers)
            .Include(x => x.DnsRecords)
            .Include(x => x.EmailForwardingRules)
            .Include(x => x.Reminders);
    }
}
