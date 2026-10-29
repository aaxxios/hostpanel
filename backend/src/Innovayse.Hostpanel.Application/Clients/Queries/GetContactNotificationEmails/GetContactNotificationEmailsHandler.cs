using Innovayse.Hostpanel.Application.Clients.Common.Enums;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetContactNotificationEmails;

/// <summary>
/// Handles <see cref="GetContactNotificationEmailsQuery"/>: the addresses of a client's additional
/// contacts who want a kind of email, each address once.
/// </summary>
public sealed class GetContactNotificationEmailsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetContactNotificationEmailsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The client and the kind of email being sent.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The distinct addresses of the contacts with that switch on, none when there are no such contacts. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<string>>> HandleAsync(GetContactNotificationEmailsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var contacts = _context.Contacts.AsNoTracking().Where(c => c.ClientId == query.ClientId);
        contacts = Wanting(contacts, query.Topic);

        IReadOnlyList<string> emails = await contacts
            .Select(c => c.Email)
            .Distinct()
            .ToListAsync(ct);

        return Result<IReadOnlyList<string>>.Success(emails);
    }

    /// <summary>Keeps the contacts whose switch for the kind of email is on.</summary>
    /// <param name="contacts">The client's contacts.</param>
    /// <param name="topic">The kind of email being sent.</param>
    /// <returns>The contacts that want that kind of email.</returns>
    private static IQueryable<Contact> Wanting(IQueryable<Contact> contacts, ContactNotification topic) => topic switch
    {
        ContactNotification.General => contacts.Where(c => c.NotifyGeneral),
        ContactNotification.Invoice => contacts.Where(c => c.NotifyInvoice),
        ContactNotification.Support => contacts.Where(c => c.NotifySupport),
        ContactNotification.Product => contacts.Where(c => c.NotifyProduct),
        ContactNotification.Domain => contacts.Where(c => c.NotifyDomain),
        ContactNotification.Affiliate => contacts.Where(c => c.NotifyAffiliate),
        _ => throw new ArgumentOutOfRangeException(nameof(topic), topic, "The validator admits only defined topics."),
    };
}
