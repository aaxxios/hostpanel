using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetInvoicePdf;

/// <summary>Renders an invoice as a PDF, with the seller read from the portal settings.</summary>
/// <remarks>
/// Serves the staff route as it is, and the client route once <c>GetMyInvoicePdfHandler</c> has
/// settled that the caller owns a published invoice. It prints whatever invoice it is given,
/// drafts included; keeping drafts from clients is the client handler's job.
/// </remarks>
public sealed class GetInvoicePdfHandler
{
    /// <summary>The PDF's content type.</summary>
    private const string PdfContentType = "application/pdf";

    /// <summary>The portal settings the seller block and logo are read from.</summary>
    private static readonly List<string> SellerKeys =
        [PortalSettingKeys.SiteName, PortalSettingKeys.ContactAddress, PortalSettingKeys.LegalTaxId, PortalSettingKeys.Logo];

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Draws the PDF.</summary>
    private readonly IInvoicePdfRenderer _renderer;

    /// <summary>The most lines an invoice may have and still be rendered.</summary>
    private readonly int _maxLines;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="renderer">Draws the PDF.</param>
    /// <param name="options">How large an invoice this deployment will render.</param>
    public GetInvoicePdfHandler(IHostpanelDbContext context, IInvoicePdfRenderer renderer, IOptions<InvoicePdfOptions> options)
    {
        _context = context;
        _renderer = renderer;
        _maxLines = options.Value.MaxLines;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The invoice to render.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The PDF as <c>invoice-{id}.pdf</c>, <c>INVOICE_NOT_FOUND</c> when the id names no invoice, or
    /// <c>INVOICE_PDF_EXCEEDS_LIMIT</c> when it has more lines than <c>InvoicePdf:MaxLines</c>.
    /// </returns>
    public async Task<Result<FileDownloadDto>> HandleAsync(GetInvoicePdfQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // One statement: the line items join in, and the client and the currency row come back as
        // correlated subqueries. AsSingleQuery() overrides the context's split-query default, which
        // would otherwise send the items as a second round trip. The items stop one past the
        // ceiling, which is enough to tell an invoice over it without reading all of its lines.
        var invoice = await _context.Invoices
            .AsNoTracking()
            .AsSingleQuery()
            .Where(i => i.Id == query.InvoiceId)
            .Select(i => new
            {
                i.Id,
                i.Status,
                i.InvoiceDate,
                i.DueDate,
                i.PaidAt,
                i.SubTotal,
                i.TaxRate,
                i.Tax,
                i.Credit,
                i.Total,
                i.Currency,
                Lines = i.Items
                    .OrderBy(item => item.Id)
                    .Take(_maxLines + 1)
                    .Select(item => new InvoicePdfLine(item.Description, item.Quantity, item.UnitPrice, item.Amount))
                    .ToList(),
                Client = _context.Clients
                    .Where(c => c.Id == i.ClientId)
                    .Select(c => new { c.FirstName, c.LastName, c.CompanyName, c.Street, c.Address2, c.City, c.State, c.PostCode, c.Country })
                    .FirstOrDefault(),
                Money = _context.Currencies
                    .Where(c => c.Code == i.Currency)
                    .Select(c => new InvoicePdfMoneyFormat(c.Prefix, c.Suffix, c.Decimals))
                    .FirstOrDefault(),
            })
            .FirstOrDefaultAsync(ct);
        if (invoice is null)
        {
            return Result<FileDownloadDto>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        if (invoice.Lines.Count > _maxLines)
        {
            return Result<FileDownloadDto>.FailureCode(HostpanelErrorCodes.INVOICE_PDF_EXCEEDS_LIMIT);
        }

        var client = invoice.Client;

        var settings = await _context.Settings
            .AsNoTracking()
            .Where(s => SellerKeys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, s => s.Value.Trim(), ct);
        string Setting(string key) => settings.GetValueOrDefault(key) ?? string.Empty;

        var sellerName = Setting(PortalSettingKeys.SiteName);
        var taxId = Setting(PortalSettingKeys.LegalTaxId);
        var sellerLines = Setting(PortalSettingKeys.ContactAddress)
            .ReplaceLineEndings("\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        if (taxId.Length > 0)
        {
            sellerLines.Add($"Tax ID: {taxId}");
        }

        var billTo = client is null
            ? new InvoicePdfParty(string.Empty, [])
            : new InvoicePdfParty(
                $"{client.FirstName} {client.LastName}".Trim(),
                NonEmpty(client.CompanyName, client.Street, client.Address2, $"{client.PostCode} {client.City}".Trim(), client.State, client.Country));

        var logo = Setting(PortalSettingKeys.Logo);
        var model = new InvoicePdfModel(
            InvoiceId: invoice.Id,
            Status: invoice.Status,
            InvoiceDate: invoice.InvoiceDate,
            DueDate: invoice.DueDate,
            PaidAt: invoice.PaidAt,
            Seller: new InvoicePdfParty(sellerName.Length > 0 ? sellerName : EmailBranding.DefaultSiteName, sellerLines),
            BillTo: billTo,
            LogoPath: logo.Length > 0 ? logo : null,
            Lines: invoice.Lines,
            SubTotal: invoice.SubTotal,
            TaxRate: invoice.TaxRate,
            Tax: invoice.Tax,
            Credit: invoice.Credit,
            Total: invoice.Total,
            Money: invoice.Money ?? InvoicePdfMoneyFormat.FallbackFor(invoice.Currency));

        var bytes = await _renderer.RenderAsync(model, ct);
        return Result<FileDownloadDto>.Success(new FileDownloadDto(bytes, PdfContentType, $"invoice-{invoice.Id}.pdf"));
    }

    /// <summary>The values that are not null or blank, trimmed, in order.</summary>
    /// <param name="values">Candidate address lines.</param>
    /// <returns>The lines worth printing.</returns>
    private static List<string> NonEmpty(params string?[] values) =>
        values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim()).ToList();
}
