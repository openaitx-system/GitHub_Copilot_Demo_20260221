#nullable enable

using ClosedXML.Excel;

namespace ExcelToPdf.Tests.TestData;

/// <summary>
/// Generates sample Excel files for testing.
/// </summary>
public static class SampleExcelGenerator
{
    /// <summary>
    /// Creates a comprehensive sample Excel file exercising all features.
    /// </summary>
    public static MemoryStream CreateComprehensiveSample()
    {
        var wb = new XLWorkbook();

        // Sheet 1: Data Types
        var ws1 = wb.AddWorksheet("Data Types");
        ws1.Cell(1, 1).Value = "Type";
        ws1.Cell(1, 2).Value = "Value";
        ws1.Cell(1, 1).Style.Font.Bold = true;
        ws1.Cell(1, 2).Style.Font.Bold = true;

        ws1.Cell(2, 1).Value = "String";
        ws1.Cell(2, 2).Value = "Hello World";

        ws1.Cell(3, 1).Value = "Number";
        ws1.Cell(3, 2).Value = 42.5;

        ws1.Cell(4, 1).Value = "DateTime";
        ws1.Cell(4, 2).Value = new DateTime(2024, 6, 15);

        ws1.Cell(5, 1).Value = "Boolean";
        ws1.Cell(5, 2).Value = true;

        ws1.Cell(6, 1).Value = "Formula";
        ws1.Cell(6, 2).FormulaA1 = "=2+2";

        ws1.Cell(7, 1).Value = "Blank";
        // Cell(7,2) left blank intentionally

        // Sheet 2: Formatting
        var ws2 = wb.AddWorksheet("Formatting");
        ws2.Cell(1, 1).Value = "Bold";
        ws2.Cell(1, 1).Style.Font.Bold = true;

        ws2.Cell(1, 2).Value = "Italic";
        ws2.Cell(1, 2).Style.Font.Italic = true;

        ws2.Cell(1, 3).Value = "Underline";
        ws2.Cell(1, 3).Style.Font.Underline = XLFontUnderlineValues.Single;

        ws2.Cell(2, 1).Value = "Red Text";
        ws2.Cell(2, 1).Style.Font.FontColor = XLColor.Red;

        ws2.Cell(2, 2).Value = "Yellow BG";
        ws2.Cell(2, 2).Style.Fill.BackgroundColor = XLColor.Yellow;

        ws2.Cell(2, 3).Value = "Large Font";
        ws2.Cell(2, 3).Style.Font.FontSize = 18;

        // Sheet 3: Line Breaks
        var ws3 = wb.AddWorksheet("Line Breaks");
        ws3.Cell(1, 1).Value = "Line1\nLine2\nLine3";
        ws3.Cell(1, 1).Style.Alignment.WrapText = true;
        ws3.Cell(1, 2).Value = "No breaks here";

        var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
