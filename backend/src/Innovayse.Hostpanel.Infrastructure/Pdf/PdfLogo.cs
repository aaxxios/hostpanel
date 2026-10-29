namespace Innovayse.Hostpanel.Infrastructure.Pdf;

/// <summary>A logo ready to place on an invoice PDF.</summary>
/// <param name="Bytes">The stored logo file. Uploads are re-encoded to PNG, but any image PDFsharp reads works.</param>
/// <param name="Width">Width to draw it at, in points, already scaled to fit.</param>
/// <param name="Height">Height to draw it at, in points, already scaled to fit.</param>
internal sealed record PdfLogo(byte[] Bytes, double Width, double Height);
