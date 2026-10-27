namespace Innovayse.Hostpanel.Application.Common.Settings;

/// <summary>How large an invoice this deployment will render as a PDF.</summary>
/// <remarks>
/// Rendering lays out every line in memory before the first page is written, so the line count is
/// what decides how long a request holds a thread and how much it allocates. The ceiling lives in
/// configuration because it is a deployment's call: an install that bills usage line by line may
/// need more than the default.
/// </remarks>
public sealed class InvoicePdfOptions
{
    /// <summary>Configuration section this binds from.</summary>
    public const string SectionName = "InvoicePdf";

    /// <summary>
    /// Most line items an invoice may have and still be rendered (<c>InvoicePdf__MaxLines</c>).
    /// An invoice over it answers <c>INVOICE_PDF_EXCEEDS_LIMIT</c> before anything is drawn.
    /// </summary>
    public int MaxLines { get; init; } = 500;
}
