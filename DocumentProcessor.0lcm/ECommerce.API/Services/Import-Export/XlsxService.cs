using System.Globalization;
using ClosedXML.Excel;
using ECommerce.API.Data;
using ECommerce.API.Interfaces.Import_Export;
using ECommerce.API.Models;
using ECommerce.Shared;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Services.Import_Export;

public class XlsxService() : IImportService, IExportService
{
    public SeedData GetSeedData(IEnumerable<string> filePaths)
    {
        using var workbook = new XLWorkbook(filePaths.First());

        var tags = GetTagsFromXlsx(workbook);
        var items = GetItemsFromXlsx(tags, workbook);
        var sales = GetSalesFromXlsx(items, workbook);

        return new SeedData
        {
            Tags = tags,
            Items = items,
            Sales = sales
        };
    }
    
    public async Task<MemoryStream> ExportDataAsync(ApiDbContext db)
    {
        var sales = await db.Sales
            .Include(s => s.SoldItems)
            .ThenInclude(si => si.Item)
            .ToListAsync();

        var items = await db.Items
            .Include(i => i.Tags)
            .ToListAsync();

        var tags = await db.Tags.ToListAsync();

        using var workbook = new XLWorkbook();

        var saleSheet = workbook.AddWorksheet("Sales");
        WriteSaleHeaders(saleSheet);
        WriteSaleValues(saleSheet, sales);

        var itemSheet = workbook.AddWorksheet("Products");
        WriteItemHeaders(itemSheet);
        WriteItemValues(itemSheet, items);

        var tagSheet = workbook.AddWorksheet("Tags");
        WriteTagHeaders(tagSheet);
        WriteTagValues(tagSheet, tags);

        saleSheet.Column(3).Style.NumberFormat.Format = "$#,##0.00";
        itemSheet.Column(8).Style.NumberFormat.Format = "$#,##0.00";

        foreach (var sheet in workbook.Worksheets)
        {
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Columns().AdjustToContents();
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        
        return stream;
    }

    //------ Import Methods -------
    private List<Tag> GetTagsFromXlsx(XLWorkbook workbook)
    {
        var range = GetRange(workbook, "Tags");
        var headers = GetHeaderDictionary(range);
        var rows = range.RowsUsed().Skip(1);

        return rows.Select(row => new Tag
        {
            TagName = row.Cell(headers["name"]).GetValue<string>()
        }).ToList();
    }

    private List<Item> GetItemsFromXlsx(List<Tag> tags, XLWorkbook workbook)
    {
        var range = GetRange(workbook, "Items");
        var headers = GetHeaderDictionary(range);

        var rows = range.RowsUsed().Skip(1);

        return rows.Select(row =>
        {
            var tagNames = row.Cell(headers["tags"]).GetValue<string>()
                .Split(',')
                .Select(n => n.Trim());

            var matchedTags = tags
                .Where(t => tagNames.Contains(t.TagName))
                .ToList();

            return new Item
            {
                Name = row.Cell(headers["name"]).GetValue<string>(),
                Artist = row.Cell(headers["artist"]).GetValue<string>(),
                Price = row.Cell(headers["price"]).GetValue<decimal>(),
                Type = GetEnumValue<ItemType>(row.Cell(headers["type"])),
                Format = GetEnumValue<ItemFormat>(row.Cell(headers["format"])),
                Genre = row.Cell(headers["genre"]).GetValue<string>(),
                Tags = matchedTags
            };
        }).ToList();
    }

    private List<Sale> GetSalesFromXlsx(List<Item> items, XLWorkbook workbook)
    {
        var range = GetRange(workbook, "Sales");
        var headers = GetHeaderDictionary(range);
        var rows = range.RowsUsed().Skip(1);

        return rows.Select(row =>
        {
            var itemEntries = row.Cell(headers["sale"]).GetValue<string>()
                .Split(',')
                .Select(entry => entry.Trim().Split(':'))
                .ToList();

            var matchedItems = itemEntries.Select(parts =>
            {
                var name = parts[0].Trim();
                var quantity = int.Parse(parts[1].Trim());
                var item = items.FirstOrDefault(i => i.Name == name);

                return new SaleItem
                {
                    Item = item!,
                    Quantity = quantity
                };
            }).Where(si => si.Item != null!).ToList();

            return new Sale
            {
                SoldItems = matchedItems
            };
        }).ToList();
    }
    
    //------- Export Methods -------
    private static void WriteSaleHeaders(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Sale ID";
        sheet.Cell(1, 2).Value = "Sold Items";
        sheet.Cell(1, 3).Value = "Total Price";
    }

    private static void WriteSaleValues(IXLWorksheet sheet, List<Sale> sales)
    {
        for (var i = 0; i < sales.Count; i++)
        {
            var sale = sales[i];
            var row = i + 2;

            sheet.Cell(row, 1).Value = sale.SaleId;
            sheet.Cell(row, 2).Value = string.Join(", ", sale.SoldItems.Select(si => $"{si.Item.Name} x {si.Quantity}"));
            sheet.Cell(row, 3).Value = sale.TotalPrice.ToString(CultureInfo.InvariantCulture);
        }
    }
    
    private static void WriteItemHeaders(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Item ID";
        sheet.Cell(1, 2).Value = "Artist";
        sheet.Cell(1, 3).Value = "Title";
        sheet.Cell(1, 4).Value = "Format";
        sheet.Cell(1, 5).Value = "Type";
        sheet.Cell(1, 6).Value = "Genre";
        sheet.Cell(1, 7).Value = "Tags";
        sheet.Cell(1, 8).Value = "Price";
    }

    private static void WriteItemValues(IXLWorksheet sheet, List<Item> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var row = i + 2;
            
            sheet.Cell(row, 1).Value = item.ItemId;
            sheet.Cell(row, 2).Value = item.Artist;
            sheet.Cell(row, 3).Value = item.Name;
            sheet.Cell(row, 4).Value = item.Format.ToString();
            sheet.Cell(row, 5).Value = item.Type.ToString();
            sheet.Cell(row, 6).Value = item.Genre;
            sheet.Cell(row, 7).Value = string.Join(", ", item.Tags.Select(t => t.TagName));
            sheet.Cell(row, 8).Value = item.Price.ToString(CultureInfo.InvariantCulture);
        }
    }

    private static void WriteTagHeaders(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Tag ID";
        sheet.Cell(1, 2).Value = "Tag Name";
    }

    private static void WriteTagValues(IXLWorksheet sheet, List<Tag> tags)
    {
        for (var i = 0; i < tags.Count; i++)
        {
            var tag = tags[i];
            var row = i + 2;
            
            sheet.Cell(row, 1).Value = tag.TagId;
            sheet.Cell(row, 2).Value = tag.TagName;
        }
    }

    //------- Helper Methods -------
    private IXLRange GetRange(XLWorkbook workbook, string worksheetName)
    {
        var sheet = workbook.Worksheet(worksheetName);
        return sheet.RangeUsed() ??
               throw new InvalidOperationException($"Extracted a null range for worksheet name {worksheetName}");
    }
    
    private T GetEnumValue<T>(IXLCell cell) where T : struct, Enum
    {
        var success = Enum.TryParse<T>(cell.GetValue<string>(), true, out T value);
        if (!success)
            throw new InvalidOperationException($"Could not parse '{cell.GetValue<string>()}' as {typeof(T).Name}");
        return value;
    }

    private Dictionary<string, int> GetHeaderDictionary(IXLRange range)
    {
        var headerRow = range.RowsUsed().First();
        return headerRow.Cells()
            .ToDictionary(
                c => c.GetValue<string>().Trim().ToLower(),
                c => c.Address.ColumnNumber);
    }
}