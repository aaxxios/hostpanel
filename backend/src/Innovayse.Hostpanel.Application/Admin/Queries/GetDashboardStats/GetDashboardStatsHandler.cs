using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Admin.Queries.GetDashboardStats;

/// <summary>
/// Handles <see cref="GetDashboardStatsQuery"/> by aggregating data from multiple sources.
/// </summary>
public sealed class GetDashboardStatsHandler
{
    /// <summary>
    /// The module's data context, for revenue, active services, open ticket and total client counts.
    /// </summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>How far back a paid invoice still counts toward the monthly figure.</summary>
    private static readonly TimeSpan MonthlyWindow = TimeSpan.FromDays(30);

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for revenue, active services, open ticket and total client counts.</param>
    public GetDashboardStatsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Computes the dashboard statistics.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the aggregated statistics. This use case refuses nothing.</returns>
    public async Task<Result<DashboardStatsDto>> HandleAsync(GetDashboardStatsQuery query, CancellationToken ct)
    {
        // Was IInvoiceRepository.GetAllAsync -- read-only, so no tracking is needed.
        var allInvoices = await _context.Invoices.AsNoTracking().ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var thirtyDaysAgo = now - MonthlyWindow;

        var totalRevenue = allInvoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .Sum(i => i.Total);

        var monthlyRevenue = allInvoices
            .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt >= thirtyDaysAgo)
            .Sum(i => i.Total);

        var overdueInvoices = allInvoices.Count(i => i.Status == InvoiceStatus.Overdue);

        // Was IClientServiceRepository.GetAllAsync -- read-only, so no tracking is needed.
        var activeServices = await _context.ClientServices
            .AsNoTracking()
            .CountAsync(s => s.Status == ServiceStatus.Active, ct);

        // Was ITicketRepository.CountByStatusAsync -- read-only, so no tracking is needed.
        var openTickets = await _context.Tickets.AsNoTracking().CountAsync(t => t.Status == TicketStatus.Open, ct);

        // Was IClientRepository.CountAllAsync.
        var totalClients = await _context.Clients.AsNoTracking().CountAsync(ct);

        var dto = new DashboardStatsDto(
            totalRevenue, monthlyRevenue, activeServices, overdueInvoices, openTickets, totalClients);
        return Result<DashboardStatsDto>.Success(dto);
    }
}
