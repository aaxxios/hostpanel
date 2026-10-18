using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Clients.Queries.ExportClientData;

/// <summary>
/// Gathers the requested sections of one client's record and returns them as the payload the
/// admin export dialog downloads.
/// <para>
/// Only the sections the caller asked for are read. Exporting an account with a long history is
/// slow enough that the dialog warns about it, so a section nobody ticked costs no query.
/// </para>
/// </summary>
public sealed class ExportClientDataHandler
{
    /// <summary>
    /// Number of activity log entries the export carries, newest first. Capped because the log is
    /// the one section with no natural ceiling.
    /// </summary>
    private const int ActivityLogLimit = 500;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the person behind the client row, for the sign-in address.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Reads the person behind the client row, for the sign-in address.</param>
    public ExportClientDataHandler(IHostpanelDbContext context, IIdentityProvider identity)
    {
        _context = context;
        _identity = identity;
    }

    /// <summary>Executes the query.</summary>
    /// <param name="query">The client to export and the sections to include.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The requested sections of the client's record, or <c>CLIENT_NOT_FOUND</c> when no such
    /// client exists.
    /// </returns>
    public async Task<Result<ClientExportDto>> HandleAsync(ExportClientDataQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var clientId = query.ClientId;

        var client = await _context.Clients
            .Include(c => c.Contacts)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clientId, ct);
        if (client is null)
        {
            return Result<ClientExportDto>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        var account = await _identity.FindBySubjectAsync(client.UserId, ct);
        var email = account?.Email ?? string.Empty;

        // Case-insensitive because the keys travel on the query string, where a caller may spell
        // them however it likes.
        var wanted = new HashSet<string>(query.Fields, StringComparer.OrdinalIgnoreCase);

        // EF Core's DbContext is not thread-safe, so the sections are read one after another.
        var profile = wanted.Contains(ClientExportSections.ProfileData)
            ? new ClientExportProfileDto(
                client.Id, client.FirstName, client.LastName, email, client.CompanyName, client.Phone,
                client.Street, client.City, client.State, client.PostCode, client.Country,
                client.Status, client.CreatedAt)
            : null;

        IReadOnlyList<ClientExportDomainDto>? domainRows = null;
        if (wanted.Contains(ClientExportSections.Domains))
        {
            domainRows = await _context.Domains.AsNoTracking()
                .Where(d => d.ClientId == clientId)
                .Select(d => new ClientExportDomainDto(d.Id, d.Name, d.Status, d.RegisteredAt, d.ExpiresAt))
                .ToListAsync(ct);
        }

        IReadOnlyList<ClientExportServiceDto>? serviceRows = null;
        if (wanted.Contains(ClientExportSections.ProductsServices))
        {
            serviceRows = await _context.ClientServices.AsNoTracking()
                .Where(s => s.ClientId == clientId)
                .Select(s => new ClientExportServiceDto(s.Id, s.ProductId, s.BillingCycle, s.Status, s.CreatedAt, s.NextRenewalAt))
                .ToListAsync(ct);
        }

        IReadOnlyList<ClientExportInvoiceDto>? invoiceRows = null;
        if (wanted.Contains(ClientExportSections.Invoices))
        {
            invoiceRows = await _context.Invoices.AsNoTracking()
                .Where(i => i.ClientId == clientId)
                .Select(i => new ClientExportInvoiceDto(i.Id, i.Status, i.Total, i.CreatedAt, i.DueDate, i.PaidAt, i.Currency))
                .ToListAsync(ct);
        }

        IReadOnlyList<ClientExportTransactionDto>? transactionRows = null;
        if (wanted.Contains(ClientExportSections.Transactions))
        {
            transactionRows = await _context.Transactions.AsNoTracking()
                .Where(t => t.ClientId == clientId)
                .OrderByDescending(t => t.Date)
                .Select(t => new ClientExportTransactionDto(t.Id, t.Date, t.Description, t.AmountIn, t.AmountOut, t.Fees, t.PaymentMethod))
                .ToListAsync(ct);
        }

        IReadOnlyList<ClientExportQuoteDto>? quoteRows = null;
        if (wanted.Contains(ClientExportSections.Quotes))
        {
            quoteRows = await _context.Quotes.AsNoTracking()
                .Where(q => q.ClientId == clientId)
                .Select(q => new ClientExportQuoteDto(q.Id, q.Stage, q.Total, q.CreatedAt, q.ExpiryDate))
                .ToListAsync(ct);
        }

        IReadOnlyList<ClientExportBillableItemDto>? billableRows = null;
        if (wanted.Contains(ClientExportSections.BillableItems))
        {
            // Every billable item on the account whatever its state -- invoiced, uninvoiced or
            // recurring alike, unlike the other repositories that only ever answer one slice.
            billableRows = await _context.BillableItems.AsNoTracking()
                .Where(b => b.ClientId == clientId)
                .Select(b => new ClientExportBillableItemDto(b.Id, b.Description, b.Amount, b.NextDueDate))
                .ToListAsync(ct);
        }

        // Contacts are part of the client aggregate, already loaded above.
        var contactRows = wanted.Contains(ClientExportSections.Contacts)
            ? client.Contacts
                .Select(c => new ClientExportContactDto(c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.Type))
                .ToList()
            : null;

        IReadOnlyList<ClientExportTicketDto>? ticketRows = null;
        if (wanted.Contains(ClientExportSections.Tickets))
        {
            ticketRows = await _context.Tickets.AsNoTracking()
                .Where(t => t.ClientId == clientId)
                .Select(t => new ClientExportTicketDto(t.Id, t.Subject, t.Status, t.Priority, t.CreatedAt))
                .ToListAsync(ct);
        }

        IReadOnlyList<ClientExportEmailDto>? emailRows = null;
        if (wanted.Contains(ClientExportSections.Emails))
        {
            // Keyed on the address the identity provider gave, not on the client id: what ties a
            // log entry to an account is who it was addressed to, and under an external SSO the
            // local user table is not where that address lives.
            emailRows = await _context.EmailLogs.AsNoTracking()
                .Where(e => e.To == email)
                .Select(e => new ClientExportEmailDto(e.Id, e.Subject, e.SentAt, e.Success))
                .ToListAsync(ct);
        }

        IReadOnlyList<ClientExportActivityDto>? activityRows = null;
        if (wanted.Contains(ClientExportSections.ActivityLog))
        {
            activityRows = await _context.ActivityLogs.AsNoTracking()
                .Where(a => a.ClientId == clientId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(ActivityLogLimit)
                .Select(a => new ClientExportActivityDto(a.Id, a.Description, a.CreatedAt, a.AdminName))
                .ToListAsync(ct);
        }

        var dto = new ClientExportDto
        {
            ProfileData = profile,
            Domains = domainRows,
            ProductsServices = serviceRows,
            Invoices = invoiceRows,
            Transactions = transactionRows,
            Quotes = quoteRows,
            BillableItems = billableRows,
            Contacts = contactRows,
            Tickets = ticketRows,
            Emails = emailRows,
            Notes = wanted.Contains(ClientExportSections.Notes) ? new ClientExportNotesDto(client.AdminNotes) : null,
            ActivityLog = activityRows,
            ConsentHistory = wanted.Contains(ClientExportSections.ConsentHistory)
                ? new ClientExportNoteDto("No consent history recorded.")
                : null,
            PayMethods = wanted.Contains(ClientExportSections.PayMethods)
                ? new ClientExportNoteDto("Payment methods are not stored locally.")
                : null,
        };

        return Result<ClientExportDto>.Success(dto);
    }
}
