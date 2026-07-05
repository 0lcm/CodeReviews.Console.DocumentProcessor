using System.Globalization;
using DocumentFormat.OpenXml.Drawing;
using ECommerce.API.Data;
using ECommerce.API.Interfaces.Import_Export;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Path = System.IO.Path;

namespace ECommerce.API.Services.Import_Export;

public class PdfService() : IExportService
{
    public async Task<MemoryStream> ExportDataAsync(ApiDbContext db)
    {
        var document = await CreateDocument(db);
        var bytes = document.GeneratePdf();

        var stream = new MemoryStream(bytes);
        stream.Position = 0;
        return stream;
    }

    private async Task<Document> CreateDocument(ApiDbContext db)
    {
        var tags = await db.Tags.ToListAsync();
        var items = await db.Items.ToListAsync();
        var sales = await db.Sales
            .Include(s => s.SoldItems)
            .ThenInclude(si => si.Item)
            .ToListAsync();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text("Sales")
                    .SemiBold().FontSize(18).FontColor(Colors.Black);

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(8);
                        columns.RelativeColumn(1);
                    });

                    static IContainer HeaderStyle(IContainer c) =>
                        c.Background(Colors.Grey.Lighten1).Padding(5);

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("Sale ID").Bold().FontColor(Colors.Black);
                        header.Cell().Element(HeaderStyle).Text("Items").Bold().FontColor(Colors.Black);
                        header.Cell().Element(HeaderStyle).Text("Price").Bold().FontColor(Colors.Black);
                    });

                    foreach (var sale in sales)
                    {
                        var soldItems = string.Join(", ", sale.SoldItems.Select(si => $"{si.Item.Name} x {si.Quantity}"));
                        static IContainer CellStyle(IContainer c) =>
                            c.BorderBottom(1).BorderColor(Colors.Black).Padding(5);
                        
                        table.Cell().Element(CellStyle).Text(sale.SaleId.ToString());
                        table.Cell().Element(CellStyle).Text(soldItems);
                        table.Cell().Element(CellStyle).Text(sale.TotalPrice.ToString(CultureInfo.InvariantCulture));
                    }
                });

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
            
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text("Products")
                    .SemiBold().FontSize(18).FontColor(Colors.Black);

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    static IContainer HeaderStyle(IContainer c) =>
                        c.Background(Colors.Grey.Lighten1).Padding(5);

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("ID").Bold().FontColor(Colors.Black);
                        header.Cell().Element(HeaderStyle).Text("Title").Bold().FontColor(Colors.Black);
                        header.Cell().Element(HeaderStyle).Text("Artist").Bold().FontColor(Colors.Black);
                        header.Cell().Element(HeaderStyle).Text("Genre").Bold().FontColor(Colors.Black);
                        header.Cell().Element(HeaderStyle).Text("Type").Bold().FontColor(Colors.Black);
                        header.Cell().Element(HeaderStyle).Text("Format").Bold().FontColor(Colors.Black);
                        header.Cell().Element(HeaderStyle).Text("Price").Bold().FontColor(Colors.Black);
                    });

                    foreach (var item in items)
                    {
                        static IContainer CellStyle(IContainer c) =>
                            c.BorderBottom(1).BorderColor(Colors.Black).Padding(5);

                        table.Cell().Element(CellStyle).Text(item.ItemId.ToString());
                        table.Cell().Element(CellStyle).Text(item.Name);
                        table.Cell().Element(CellStyle).Text(item.Artist);
                        table.Cell().Element(CellStyle).Text(item.Genre);
                        table.Cell().Element(CellStyle).Text(item.Type.ToString());
                        table.Cell().Element(CellStyle).Text(item.Format.ToString());
                        table.Cell().Element(CellStyle).Text(item.Price.ToString(CultureInfo.InvariantCulture));
                    }
                });

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text("Tags")
                    .SemiBold().FontSize(18).FontColor(Colors.Black);

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(9);
                    });

                    static IContainer HeaderStyle(IContainer c) =>
                        c.Background(Colors.Grey.Lighten1).Padding(5);

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("Tag ID").Bold().FontColor(Colors.Black);
                        header.Cell().Element(HeaderStyle).Text("Name").Bold().FontColor(Colors.Black);
                    });

                    foreach (var tag in tags)
                    {
                        static IContainer CellStyle(IContainer c) =>
                            c.BorderBottom(1).BorderColor(Colors.Black).Padding(5);
                        
                        table.Cell().Element(CellStyle).Text(tag.TagId.ToString());
                        table.Cell().Element(CellStyle).Text(tag.TagName);
                    }
                });

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });
    }
}