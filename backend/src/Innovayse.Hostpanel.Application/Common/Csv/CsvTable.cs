using System.Globalization;
using System.Text;

namespace Innovayse.Hostpanel.Application.Common.Csv;

/// <summary>
/// The one CSV writer every export uses: a header row, then data rows, rendered as RFC 4180 text.
/// Numbers and dates are written in the invariant culture, so a file never depends on the server's
/// locale; a text cell holding a comma, a quote or a line break is quoted with embedded quotes
/// doubled; and a text cell that a
/// spreadsheet would run as a formula is defused.
/// </summary>
public sealed class CsvTable
{
    /// <summary>The MIME type of the rendered file.</summary>
    private const string CsvContentType = "text/csv";

    /// <summary>The row separator RFC 4180 names.</summary>
    private const string RowSeparator = "\r\n";

    /// <summary>
    /// Leading characters a spreadsheet treats as the start of a formula (plus the tab and carriage
    /// return some of them skip before looking). A text cell starting with one gets a leading
    /// apostrophe, so client-entered text such as a name or a note can never run as a formula.
    /// </summary>
    private static readonly char[] FormulaLeads = ['=', '+', '-', '@', '\t', '\r'];

    /// <summary>Characters that force a text cell into quotes, per RFC 4180.</summary>
    private static readonly char[] QuoteTriggers = [',', '"', '\r', '\n'];

    /// <summary>The rendered text so far.</summary>
    private readonly StringBuilder _text = new();

    /// <summary>The number of cells every row must have.</summary>
    private readonly int _width;

    /// <summary>Initializes the table with its header row.</summary>
    /// <param name="header">The column titles, in column order.</param>
    public CsvTable(IReadOnlyList<string> header)
    {
        ArgumentNullException.ThrowIfNull(header);
        _width = header.Count;
        AppendRow(header.Cast<object?>().ToList());
    }

    /// <summary>Adds one data row.</summary>
    /// <param name="cells">
    /// The cells, in column order: text, numbers, dates or null (written as an empty cell). Numbers
    /// are written unquoted so a negative amount stays a number.
    /// </param>
    /// <exception cref="ArgumentException">The row does not have as many cells as the header.</exception>
    public void AddRow(IReadOnlyList<object?> cells)
    {
        ArgumentNullException.ThrowIfNull(cells);
        if (cells.Count != _width)
        {
            throw new ArgumentException($"A row needs {_width} cells; got {cells.Count}.", nameof(cells));
        }

        AppendRow(cells);
    }

    /// <summary>Renders the table as a UTF-8 CSV download.</summary>
    /// <param name="fileName">The suggested download name, with extension.</param>
    /// <returns>The file, ready for <c>BaseApiController.FileResult</c>.</returns>
    public FileDownloadDto ToFile(string fileName) =>
        new(Encoding.UTF8.GetBytes(_text.ToString()), CsvContentType, fileName);

    /// <summary>Writes one row, cells separated by commas, ended by the row separator.</summary>
    /// <param name="cells">The cells to write.</param>
    private void AppendRow(IReadOnlyList<object?> cells)
    {
        for (var i = 0; i < cells.Count; i++)
        {
            if (i > 0)
            {
                _text.Append(',');
            }

            _text.Append(Format(cells[i]));
        }

        _text.Append(RowSeparator);
    }

    /// <summary>Renders one cell.</summary>
    /// <param name="cell">The cell value.</param>
    /// <returns>The cell as CSV text.</returns>
    private static string Format(object? cell) => cell switch
    {
        null => string.Empty,
        string text => FormatText(text),
        decimal or double or float or int or long or short or byte =>
            ((IFormattable)cell).ToString(null, CultureInfo.InvariantCulture),
        IFormattable other => FormatText(other.ToString(null, CultureInfo.InvariantCulture)),
        _ => FormatText(cell.ToString() ?? string.Empty),
    };

    /// <summary>
    /// Renders a text cell: defuses a formula lead, then quotes the cell, embedded quotes doubled,
    /// when it holds a comma, a quote or a line break.
    /// </summary>
    /// <param name="text">The raw cell value.</param>
    /// <returns>The cell as CSV text.</returns>
    private static string FormatText(string text)
    {
        var safe = text.Length > 0 && FormulaLeads.Contains(text[0]) ? "'" + text : text;
        if (safe.IndexOfAny(QuoteTriggers) < 0)
        {
            return safe;
        }

        return "\"" + safe.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}
