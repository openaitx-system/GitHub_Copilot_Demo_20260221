#nullable enable

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ClosedXML.Excel;
using ExcelToPdf.Core.Models;
using ExcelToPdf.Parsing;
using ExcelToPdf.Rendering;
using Microsoft.Extensions.Logging.Abstractions;

namespace ExcelToPdf.Benchmarks;

/// <summary>
/// Benchmarks for Excel-to-PDF conversion pipeline.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ConversionBenchmarks
{
    private byte[] _smallExcel = null!;
    private byte[] _mediumExcel = null!;
    private byte[] _largeExcel = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallExcel = CreateExcel(5, 3);     // 5 rows × 3 columns
        _mediumExcel = CreateExcel(50, 10);  // 50 rows × 10 columns
        _largeExcel = CreateExcel(500, 20);  // 500 rows × 20 columns
    }

    [Benchmark(Baseline = true)]
    public async Task SmallWorkbook_5x3()
    {
        await ConvertAsync(_smallExcel);
    }

    [Benchmark]
    public async Task MediumWorkbook_50x10()
    {
        await ConvertAsync(_mediumExcel);
    }

    [Benchmark]
    public async Task LargeWorkbook_500x20()
    {
        await ConvertAsync(_largeExcel);
    }

    [Benchmark]
    public async Task ParseOnly_MediumWorkbook()
    {
        var parser = new ExcelParser(NullLogger<ExcelParser>.Instance);
        using var stream = new MemoryStream(_mediumExcel);
        await parser.ParseAsync(stream);
    }

    [Benchmark]
    public async Task RenderOnly_MediumWorkbook()
    {
        var parser = new ExcelParser(NullLogger<ExcelParser>.Instance);
        var renderer = new PdfRenderer(NullLogger<PdfRenderer>.Instance);

        using var excelStream = new MemoryStream(_mediumExcel);
        var worksheets = await parser.ParseAsync(excelStream);

        using var pdfStream = new MemoryStream();
        await renderer.RenderAsync(worksheets, pdfStream, new ConversionOptions());
    }

    private static async Task ConvertAsync(byte[] excelData)
    {
        var parser = new ExcelParser(NullLogger<ExcelParser>.Instance);
        var renderer = new PdfRenderer(NullLogger<PdfRenderer>.Instance);
        var converter = new ExcelToPdfConverter(
            parser, renderer, NullLogger<ExcelToPdfConverter>.Instance);

        using var excelStream = new MemoryStream(excelData);
        using var pdfStream = new MemoryStream();
        await converter.ConvertAsync(excelStream, pdfStream);
    }

    private static byte[] CreateExcel(int rows, int cols)
    {
        var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Benchmark");

        // Header
        for (int col = 1; col <= cols; col++)
        {
            ws.Cell(1, col).Value = $"Column {col}";
            ws.Cell(1, col).Style.Font.Bold = true;
        }

        // Data
        for (int row = 2; row <= rows; row++)
        {
            for (int col = 1; col <= cols; col++)
            {
                if (col % 3 == 0)
                    ws.Cell(row, col).Value = row * col * 1.5;
                else if (col % 3 == 1)
                    ws.Cell(row, col).Value = $"Data R{row}C{col}";
                else
                    ws.Cell(row, col).Value = new DateTime(2024, 1, 1).AddDays(row);
            }
        }

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }
}
