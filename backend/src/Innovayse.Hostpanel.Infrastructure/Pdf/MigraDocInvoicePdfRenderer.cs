using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Infrastructure.Storage.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Drawing;

namespace Innovayse.Hostpanel.Infrastructure.Pdf;

/// <summary>
/// Lays invoices out as A4 PDFs with MigraDoc, rendered through PDFsharp.
/// </summary>
/// <remarks>
/// <para>
/// MigraDoc does the layout: wrapping, the items table with its header repeated on every page it
/// runs onto, page breaks and the page-number footer. The PAID or DRAFT stamp is the one thing
/// drawn afterwards, straight onto the first page with PDFsharp, because MigraDoc cannot rotate
/// text. Text is set as text, so the PDF stays searchable and selectable.
/// </para>
/// <para>
/// The logo is read only from the branding upload directory under the web root. A
/// <c>portal.logo</c> that is a URL, or a path that leads anywhere else, is left out: rendering
/// never reaches the network and never opens a file outside that directory.
/// </para>
/// </remarks>
public sealed partial class MigraDocInvoicePdfRenderer : IInvoicePdfRenderer
{
    /// <summary>A4 width in points.</summary>
    private const double PageWidth = 595;

    /// <summary>A4 height in points.</summary>
    private const double PageHeight = 842;

    /// <summary>Margin on the top and sides, in points.</summary>
    private const double Margin = 40;

    /// <summary>Space kept above the bottom margin for the page number.</summary>
    private const double FooterHeight = 20;

    /// <summary>Size of body text.</summary>
    private const double BodySize = 9;

    /// <summary>Size of labels and the page number.</summary>
    private const double SmallSize = 8;

    /// <summary>Size of party names and the total.</summary>
    private const double EmphasisSize = 11;

    /// <summary>Size of the invoice number in the header.</summary>
    private const double TitleSize = 18;

    /// <summary>Size of the PAID or DRAFT stamp.</summary>
    private const double StampSize = 40;

    /// <summary>Rotation of the stamp, in degrees.</summary>
    private const double StampAngle = -20;

    /// <summary>Vertical centre of the stamp: across the table on a typical first page.</summary>
    private const double StampCentreY = PageHeight * 0.385;

    /// <summary>Thickness of table rules.</summary>
    private const double RuleWidth = 0.75;

    /// <summary>Space below the logo and between the totals rule and the total.</summary>
    private const double RuleGap = 6;

    /// <summary>Space above and below the text of a table row.</summary>
    private const double RowPadding = 3;

    /// <summary>Space between the header and the bill-to block.</summary>
    private const double SectionGap = 24;

    /// <summary>Space between the bill-to block and the items table.</summary>
    private const double TableGap = 20;

    /// <summary>Space between the items table and the totals.</summary>
    private const double TotalsGap = 12;

    /// <summary>Space between the invoice number and the dates below it.</summary>
    private const double TitleGap = 8;

    /// <summary>Width kept for a date or status value; its label sits to the left of it.</summary>
    private const double HeaderValueWidth = 80;

    /// <summary>Width of the invoice number, dates and status block on the right of the header.</summary>
    private const double HeaderRightBlockWidth = 200;

    /// <summary>Width of the page between the margins.</summary>
    private const double ContentWidth = PageWidth - (2 * Margin);

    /// <summary>Width of the Qty column.</summary>
    private const double QtyWidth = 50;

    /// <summary>Width of each money column.</summary>
    private const double MoneyWidth = 90;

    /// <summary>Gap kept between columns: description and Qty, seller and header block.</summary>
    private const double ColumnGap = 8;

    /// <summary>
    /// Width of the description column, the narrowest that customer-supplied text goes into. A
    /// word wider than this cannot fit on a line anywhere it is printed.
    /// </summary>
    private const double DescriptionWidth = ContentWidth - QtyWidth - (2 * MoneyWidth) - ColumnGap;

    /// <summary>Shortest word worth measuring: nothing shorter is near <see cref="DescriptionWidth"/>.</summary>
    private const int LongWordLength = 20;

    /// <summary>A break opportunity that prints nothing; MigraDoc breaks a line there when it must.</summary>
    private const char ZeroWidthSpace = '\u200B';

    /// <summary>Width of the totals block at the bottom right.</summary>
    private const double TotalsWidth = 220;

    /// <summary>Largest width the logo is drawn at.</summary>
    private const double LogoMaxWidth = 160;

    /// <summary>Largest height the logo is drawn at.</summary>
    private const double LogoMaxHeight = 60;

    /// <summary>The date format every date on the invoice uses.</summary>
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>Text colour.</summary>
    private static readonly Color Ink = new(0x1F, 0x29, 0x37);

    /// <summary>Colour of labels and secondary text.</summary>
    private static readonly Color Muted = new(0x6B, 0x72, 0x80);

    /// <summary>Colour of table rules.</summary>
    private static readonly Color Rule = new(0xD1, 0xD5, 0xDB);

    /// <summary>Colour of the PAID stamp, translucent so the page shows through.</summary>
    private static readonly XColor PaidStamp = XColor.FromArgb(0x55, 0x16, 0xA3, 0x4A);

    /// <summary>Colour of the DRAFT stamp, translucent so the page shows through.</summary>
    private static readonly XColor DraftStamp = XColor.FromArgb(0x55, 0x6B, 0x72, 0x80);

    /// <summary>Where the web root is, for finding an uploaded logo.</summary>
    private readonly IWebHostEnvironment _env;

    /// <summary>Where branding uploads live under the web root.</summary>
    private readonly BrandingOptions _branding;

    /// <summary>Logs why a configured logo was left out.</summary>
    private readonly ILogger<MigraDocInvoicePdfRenderer> _logger;

    /// <summary>Initializes the renderer.</summary>
    /// <param name="env">Where the web root is, for finding an uploaded logo.</param>
    /// <param name="branding">Where branding uploads live under the web root.</param>
    /// <param name="logger">Logs why a configured logo was left out.</param>
    public MigraDocInvoicePdfRenderer(
        IWebHostEnvironment env,
        IOptions<BrandingOptions> branding,
        ILogger<MigraDocInvoicePdfRenderer> logger)
    {
        _env = env;
        _branding = branding.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layout and rendering are synchronous CPU work, so they run on the thread pool rather than on
    /// the request's thread.
    /// </remarks>
    public Task<byte[]> RenderAsync(InvoicePdfModel model, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(model);

        return Task.Run(() => Render(model, ct), ct);
    }

    /// <summary>Lays out and renders the invoice.</summary>
    /// <param name="model">Everything the PDF prints, already resolved.</param>
    /// <param name="ct">Cancellation token, checked before the work starts.</param>
    /// <returns>The PDF file's bytes.</returns>
    private byte[] Render(InvoicePdfModel model, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        PdfFonts.EnsureInitialized();

        var document = new Document();
        document.Info.Title = $"Invoice #{model.InvoiceId}";
        document.Info.Author = model.Seller.Name;
        var normal = document.Styles[StyleNames.Normal]!;
        normal.Font.Name = PdfFonts.Primary;
        normal.Font.Size = BodySize;
        normal.Font.Color = Ink;

        var section = document.AddSection();
        var setup = section.PageSetup;
        setup.PageWidth = PageWidth;
        setup.PageHeight = PageHeight;
        setup.TopMargin = Margin;
        setup.LeftMargin = Margin;
        setup.RightMargin = Margin;
        setup.BottomMargin = Margin + FooterHeight;
        setup.FooterDistance = Margin - SmallSize;

        AddHeader(section, model, LoadLogo(model.LogoPath));
        AddParty(section.Elements, "Bill to", model.BillTo).Format.SpaceBefore = SectionGap;
        AddItems(section, model);
        AddTotals(section, model);
        AddFooter(section);

        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();
        using var pdf = renderer.PdfDocument;
        DrawStamp(pdf.Pages[0], model);

        using var output = new MemoryStream();
        pdf.Save(output, false);
        return output.ToArray();
    }

    /// <summary>
    /// Adds a paragraph of text, setting any character the main font lacks in the fallback font.
    /// </summary>
    /// <param name="elements">Where the paragraph goes: a section's or a cell's elements.</param>
    /// <param name="text">The text.</param>
    /// <param name="bold">Whether it is set in bold.</param>
    /// <param name="size">Font size in points.</param>
    /// <param name="color">Colour; the body text colour when null.</param>
    /// <param name="alignment">Horizontal alignment.</param>
    /// <returns>The paragraph, for further formatting.</returns>
    private static Paragraph AddText(
        DocumentElements elements,
        string text,
        bool bold = false,
        double size = BodySize,
        Color? color = null,
        ParagraphAlignment alignment = ParagraphAlignment.Left)
    {
        var paragraph = elements.AddParagraph();
        paragraph.Format.Font.Bold = bold;
        paragraph.Format.Font.Size = size;
        paragraph.Format.Font.Color = color ?? Ink;
        paragraph.Format.Alignment = alignment;
        foreach (var (run, useFallback) in PdfFonts.Runs(BreakLongWords(text, bold, size), bold))
        {
            var formatted = paragraph.AddFormattedText(run);
            if (useFallback)
            {
                formatted.Font.Name = PdfFonts.Fallback;
            }
        }

        return paragraph;
    }

    /// <summary>
    /// Lets MigraDoc break a word too wide for any column: it only breaks lines at spaces, so a
    /// long unbroken word (a hostname, a pasted reference) would otherwise run off the page.
    /// </summary>
    /// <remarks>
    /// Such a word gets a zero-width space between its characters, never before a combining mark.
    /// Words that fit are left alone, so they still wrap whole.
    /// </remarks>
    /// <param name="text">The text.</param>
    /// <param name="bold">Whether it is set in bold.</param>
    /// <param name="size">Font size in points.</param>
    /// <returns>The text, with break opportunities inside any word too wide to fit.</returns>
    private static string BreakLongWords(string text, bool bold, double size)
    {
        var words = text.Split(' ');
        if (words.All(w => w.Length < LongWordLength))
        {
            return text;
        }

        using var measure = XGraphics.CreateMeasureContext(new XSize(PageWidth, PageHeight), XGraphicsUnit.Point, XPageDirection.Downwards);
        var font = new XFont(PdfFonts.Primary, size, bold ? XFontStyleEx.Bold : XFontStyleEx.Regular);
        for (var i = 0; i < words.Length; i++)
        {
            if (words[i].Length < LongWordLength || measure.MeasureString(words[i], font).Width <= DescriptionWidth)
            {
                continue;
            }

            var breakable = new StringBuilder();
            foreach (var rune in words[i].EnumerateRunes())
            {
                var category = Rune.GetUnicodeCategory(rune);
                if (breakable.Length > 0 && category is not (UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.EnclosingMark))
                {
                    breakable.Append(ZeroWidthSpace);
                }

                breakable.Append(rune.ToString());
            }

            words[i] = breakable.ToString();
        }

        return string.Join(' ', words);
    }

    /// <summary>
    /// Adds the logo and seller on the left and the invoice number, dates and status on the right,
    /// as one borderless table row so a long seller address wraps beside the dates instead of over
    /// them, and whatever follows starts below the taller side.
    /// </summary>
    /// <param name="section">The section being built.</param>
    /// <param name="model">The invoice.</param>
    /// <param name="logo">The logo's PNG bytes and size in points, or null for none.</param>
    private static void AddHeader(Section section, InvoicePdfModel model, (byte[] Bytes, double Width, double Height)? logo)
    {
        var table = section.AddTable();
        table.LeftPadding = 0;
        table.RightPadding = 0;
        table.TopPadding = 0;
        table.BottomPadding = 0;
        table.AddColumn(ContentWidth - HeaderRightBlockWidth).RightPadding = ColumnGap;
        table.AddColumn(HeaderRightBlockWidth);
        var row = table.AddRow();

        var seller = row.Cells[0];
        if (logo is { } image)
        {
            var picture = seller.AddImage("base64:" + Convert.ToBase64String(image.Bytes));
            picture.LockAspectRatio = false;
            picture.Width = image.Width;
            picture.Height = image.Height;
        }

        AddParty(seller.Elements, null, model.Seller, firstSpaceBefore: logo is null ? 0 : RuleGap);

        var details = row.Cells[1].Elements;
        AddText(details, $"Invoice #{model.InvoiceId}", bold: true, TitleSize, alignment: ParagraphAlignment.Right)
            .Format.SpaceAfter = TitleGap;
        foreach (var (label, value) in new[]
        {
            ("Invoice date", model.InvoiceDate.ToString(DateFormat, CultureInfo.InvariantCulture)),
            ("Due date", model.DueDate.ToString(DateFormat, CultureInfo.InvariantCulture)),
            ("Status", StatusText(model.Status.ToString())),
        })
        {
            // Right-aligned tab stops: the label ends where the value column starts, the value at the edge.
            var paragraph = details.AddParagraph();
            paragraph.Format.TabStops.AddTabStop(HeaderRightBlockWidth - HeaderValueWidth, TabAlignment.Right);
            paragraph.Format.TabStops.AddTabStop(HeaderRightBlockWidth, TabAlignment.Right);
            paragraph.AddTab();
            paragraph.AddFormattedText(label + ":").Color = Muted;
            paragraph.AddTab();
            paragraph.AddText(value);
        }
    }

    /// <summary>Adds a party: an optional label, the name in bold, then its lines.</summary>
    /// <param name="elements">Where the paragraphs go.</param>
    /// <param name="label">A heading above the name, or null for none.</param>
    /// <param name="party">The party.</param>
    /// <param name="firstSpaceBefore">Space above the first paragraph.</param>
    /// <returns>The first paragraph added, for spacing it from what comes before.</returns>
    private static Paragraph AddParty(DocumentElements elements, string? label, InvoicePdfParty party, double firstSpaceBefore = 0)
    {
        var first = label is null
            ? AddText(elements, party.Name, bold: true, EmphasisSize)
            : AddText(elements, label, bold: true, SmallSize, Muted);
        first.Format.SpaceBefore = firstSpaceBefore;
        if (label is not null)
        {
            AddText(elements, party.Name, bold: true, EmphasisSize);
        }

        foreach (var line in party.Lines)
        {
            AddText(elements, line);
        }

        return first;
    }

    /// <summary>Adds the items table, its header row repeated on every page it runs onto.</summary>
    /// <param name="section">The section being built.</param>
    /// <param name="model">The invoice.</param>
    private static void AddItems(Section section, InvoicePdfModel model)
    {
        var table = section.AddTable();
        table.LeftPadding = 0;
        table.RightPadding = 0;
        table.TopPadding = RowPadding;
        table.BottomPadding = RowPadding;
        table.Borders.Bottom.Width = RuleWidth;
        table.Borders.Bottom.Color = Rule;
        table.AddColumn(ContentWidth - QtyWidth - (2 * MoneyWidth)).RightPadding = ColumnGap;
        table.AddColumn(QtyWidth);
        table.AddColumn(MoneyWidth);
        table.AddColumn(MoneyWidth);

        var header = table.AddRow();
        header.HeadingFormat = true;
        header.TopPadding = TableGap;
        AddText(header.Cells[0].Elements, "Description", bold: true, color: Muted);
        AddText(header.Cells[1].Elements, "Qty", bold: true, color: Muted, alignment: ParagraphAlignment.Right);
        AddText(header.Cells[2].Elements, "Unit price", bold: true, color: Muted, alignment: ParagraphAlignment.Right);
        AddText(header.Cells[3].Elements, "Amount", bold: true, color: Muted, alignment: ParagraphAlignment.Right);

        foreach (var line in model.Lines)
        {
            var row = table.AddRow();
            AddText(row.Cells[0].Elements, line.Description);
            AddText(row.Cells[1].Elements, line.Quantity.ToString(CultureInfo.InvariantCulture), alignment: ParagraphAlignment.Right);
            AddText(row.Cells[2].Elements, model.Money.Format(line.UnitPrice), alignment: ParagraphAlignment.Right);
            AddText(row.Cells[3].Elements, model.Money.Format(line.Amount), alignment: ParagraphAlignment.Right);
        }
    }

    /// <summary>Adds subtotal, tax, credit when there is any, and the total, kept together on one page.</summary>
    /// <param name="section">The section being built.</param>
    /// <param name="model">The invoice.</param>
    private static void AddTotals(Section section, InvoicePdfModel model)
    {
        var rows = new List<(string Label, string Value)>
        {
            ("Subtotal", model.Money.Format(model.SubTotal)),
            ($"Tax ({model.TaxRate.ToString("0.##", CultureInfo.InvariantCulture)}%)", model.Money.Format(model.Tax)),
        };
        if (model.Credit > 0)
        {
            rows.Add(("Credit", "-" + model.Money.Format(model.Credit)));
        }

        var table = section.AddTable();
        table.LeftPadding = 0;
        table.RightPadding = 0;
        table.TopPadding = 0;
        table.BottomPadding = 0;
        table.Rows.LeftIndent = ContentWidth - TotalsWidth;
        table.AddColumn(TotalsWidth - MoneyWidth);
        table.AddColumn(MoneyWidth);

        foreach (var (label, value) in rows)
        {
            var row = table.AddRow();
            AddText(row.Cells[0].Elements, label, color: Muted);
            AddText(row.Cells[1].Elements, value, alignment: ParagraphAlignment.Right);
        }

        table.Rows[0].TopPadding = TotalsGap;
        table.Rows[rows.Count - 1].BottomPadding = RuleGap;
        table.Rows[rows.Count - 1].Borders.Bottom.Width = RuleWidth;
        table.Rows[rows.Count - 1].Borders.Bottom.Color = Rule;

        var total = table.AddRow();
        total.TopPadding = RuleGap;
        AddText(total.Cells[0].Elements, "Total", bold: true, EmphasisSize);
        AddText(total.Cells[1].Elements, model.Money.Format(model.Total), bold: true, EmphasisSize, alignment: ParagraphAlignment.Right);

        table.Rows[0].KeepWith = rows.Count;
    }

    /// <summary>Adds "Page n of m", centred in the bottom margin of every page.</summary>
    /// <param name="section">The section being built.</param>
    private static void AddFooter(Section section)
    {
        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Alignment = ParagraphAlignment.Center;
        footer.Format.Font.Size = SmallSize;
        footer.Format.Font.Color = Muted;
        footer.AddText("Page ");
        footer.AddPageField();
        footer.AddText(" of ");
        footer.AddNumPagesField();
    }

    /// <summary>Stamps the first page PAID (with the date) or DRAFT; other statuses get no stamp.</summary>
    /// <param name="page">The rendered first page.</param>
    /// <param name="model">The invoice.</param>
    private static void DrawStamp(PdfSharp.Pdf.PdfPage page, InvoicePdfModel model)
    {
        var (text, color) = model.Status switch
        {
            InvoiceStatus.Paid => (
                model.PaidAt is { } paidAt ? $"PAID {paidAt.ToString(DateFormat, CultureInfo.InvariantCulture)}" : "PAID",
                PaidStamp),
            InvoiceStatus.Draft => ("DRAFT", DraftStamp),
            _ => (null, XColors.Transparent),
        };
        if (text is null)
        {
            return;
        }

        using var graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
        var centre = new XPoint(PageWidth / 2, StampCentreY);
        graphics.RotateAtTransform(StampAngle, centre);
        graphics.DrawString(
            text,
            new XFont(PdfFonts.Primary, StampSize, XFontStyleEx.Bold),
            new XSolidBrush(color),
            centre,
            XStringFormats.Center);
    }

    /// <summary>Turns an enum name into words: <c>PaymentPending</c> becomes <c>Payment pending</c>.</summary>
    /// <param name="name">The enum member's name.</param>
    /// <returns>The words, first letter capitalised.</returns>
    private static string StatusText(string name) =>
        WordBoundary().Replace(name, m => " " + m.Value.ToLowerInvariant());

    /// <summary>An upper-case letter that starts a new word inside an enum name.</summary>
    /// <returns>The compiled expression.</returns>
    [GeneratedRegex("(?<=[a-z])[A-Z]")]
    private static partial Regex WordBoundary();

    /// <summary>
    /// Reads the logo when <paramref name="logoPath"/> names an image inside the branding upload
    /// directory; otherwise logs why at Debug and returns null.
    /// </summary>
    /// <param name="logoPath">The <c>portal.logo</c> value as stored.</param>
    /// <returns>The image bytes and the size to draw it at, or null to print without a logo.</returns>
    private (byte[] Bytes, double Width, double Height)? LoadLogo(string? logoPath)
    {
        if (string.IsNullOrWhiteSpace(logoPath))
        {
            return null;
        }

        if (Uri.TryCreate(logoPath, UriKind.Absolute, out var uri) && uri.Scheme is not "file")
        {
            _logger.LogDebug("Invoice PDF logo skipped: {LogoPath} is not a local upload.", logoPath);
            return null;
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var baseDir = Path.GetFullPath(Path.Combine(webRoot, _branding.RelativePath)) + Path.DirectorySeparatorChar;
        var relative = logoPath.Split('?', 2)[0].TrimStart('/');
        var full = Path.GetFullPath(Path.Combine(webRoot, relative));
        if (!full.StartsWith(baseDir, StringComparison.Ordinal) || !File.Exists(full))
        {
            _logger.LogDebug("Invoice PDF logo skipped: {LogoPath} is not a file in the branding directory.", logoPath);
            return null;
        }

        try
        {
            var bytes = File.ReadAllBytes(full);
            using var image = XImage.FromStream(new MemoryStream(bytes));
            var scale = Math.Min(1, Math.Min(LogoMaxWidth / image.PixelWidth, LogoMaxHeight / image.PixelHeight));
            return (bytes, image.PixelWidth * scale, image.PixelHeight * scale);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            _logger.LogDebug(ex, "Invoice PDF logo skipped: {LogoPath} could not be read or decoded.", logoPath);
            return null;
        }
    }
}
