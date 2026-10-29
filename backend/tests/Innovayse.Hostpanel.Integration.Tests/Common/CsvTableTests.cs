using System.Globalization;
using System.Text;
using Innovayse.Hostpanel.Application.Common.Csv;

namespace Innovayse.Hostpanel.Integration.Tests.Common;

/// <summary>
/// <see cref="CsvTable"/>: the one CSV writer every export uses. Quoting, the invariant number
/// format and the formula guard are checked on the rendered bytes.
/// </summary>
public sealed class CsvTableTests
{
    /// <summary>A plain header and row are written unquoted, comma separated, each row ended by CRLF.</summary>
    [Fact]
    public void ToFile_PlainCells_WritesThemUnquoted()
    {
        var table = new CsvTable(["ID", "Name"]);
        table.AddRow([1, "Ada"]);

        Render(table).Should().Be("ID,Name\r\n1,Ada\r\n");
    }

    /// <summary>A cell holding a comma, a quote or a line break is quoted, embedded quotes doubled.</summary>
    [Fact]
    public void ToFile_CellWithSeparatorOrQuote_IsQuotedAndEscaped()
    {
        var table = new CsvTable(["Notes"]);
        table.AddRow(["Paid, \"in full\""]);
        table.AddRow(["two\nlines"]);

        Render(table).Should().Be("Notes\r\n\"Paid, \"\"in full\"\"\"\r\n\"two\nlines\"\r\n");
    }

    /// <summary>Text a spreadsheet would run as a formula gets a leading apostrophe; numbers, negative ones included, do not.</summary>
    [Fact]
    public void ToFile_FormulaLeads_AreDefusedInTextOnly()
    {
        var table = new CsvTable(["A", "B", "C", "D", "E"]);
        table.AddRow(["=SUM(A1)", "+1", "-2", "@cmd", -5.5m]);

        Render(table).Should().Be("A,B,C,D,E\r\n'=SUM(A1),'+1,'-2,'@cmd,-5.5\r\n");
    }

    /// <summary>Numbers use the invariant decimal point whatever the thread culture, and null is an empty cell.</summary>
    [Fact]
    public void ToFile_UnderACommaDecimalCulture_WritesAPoint()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
        try
        {
            var table = new CsvTable(["Amount", "Invoice"]);
            table.AddRow([1234.5m, null]);

            Render(table).Should().Be("Amount,Invoice\r\n1234.5,\r\n");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>A row with fewer cells than the header is refused rather than shifting the columns.</summary>
    [Fact]
    public void AddRow_WrongWidth_Throws()
    {
        var table = new CsvTable(["A", "B"]);

        var act = () => table.AddRow(["only one"]);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>The file is named as asked and typed <c>text/csv</c>.</summary>
    [Fact]
    public void ToFile_NamesAndTypesTheFile()
    {
        var file = new CsvTable(["A"]).ToFile("report.csv");

        file.FileName.Should().Be("report.csv");
        file.ContentType.Should().Be("text/csv");
    }

    /// <summary>Renders a table and decodes it back to text.</summary>
    /// <param name="table">The table to render.</param>
    /// <returns>The CSV text.</returns>
    private static string Render(CsvTable table) => Encoding.UTF8.GetString(table.ToFile("t.csv").FileContent);
}
