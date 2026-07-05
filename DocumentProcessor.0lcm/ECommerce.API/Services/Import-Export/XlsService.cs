using ECommerce.API.Interfaces.Import_Export;
using ECommerce.API.Models;
using ECommerce.Shared;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace ECommerce.API.Services.Import_Export;

public class XlsService : IImportService
{
    public SeedData GetSeedData(IEnumerable<string> filePaths)
    {
        var filePath = filePaths.First();
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var workbook = new HSSFWorkbook(stream);

        var tags = GetTags(workbook);
        var items = GetItems(workbook, tags);
        var sales = GetSales(workbook, items);

        return new SeedData
        {
            Tags = tags,
            Items = items,
            Sales = sales
        };
    }
    
    //------- Extractor Methods -------
    private List<Tag> GetTags(HSSFWorkbook workbook)
    {
        var sheet = workbook.GetSheet("Tags")
                    ?? throw new InvalidOperationException("Could not find .xls sheet with the name 'Tags'");
        var headers = GetHeaders(sheet.GetRow(0));
        var tags = new List<Tag>();

        for (var i = 1; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row is null) continue;
            
            tags.Add(new Tag
            {
                TagName = GetStringValue(row.GetCell(headers["Name"]))
            });
        }

        return tags;
    }

    private List<Item> GetItems(HSSFWorkbook workbook, List<Tag> tags)
    {
        var sheet = workbook.GetSheet("Items")
            ?? throw new InvalidOperationException("Could not find .xls sheet with the name 'Items'");
        var headers = GetHeaders(sheet.GetRow(0));
        var items = new List<Item>();

        for (var i = 1; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row is null) continue;

            var tagNames = GetStringValue(row.GetCell(headers["Tags"]))
                .Split(',')
                .Select(t => t.Trim())
                .ToList();
            
            var matchedTags = tags
                .Where(t => tagNames.Contains(t.TagName))
                .ToList();
            
            items.Add(new Item
            {
                Name = GetStringValue(row.GetCell(headers["Name"])),
                Artist = GetStringValue(row.GetCell(headers["Artist"])),
                Price = GetDecimalValue(row.GetCell(headers["Price"])),
                Type = GetEnumValue<ItemType>(row.GetCell(headers["Type"])),
                Format = GetEnumValue<ItemFormat>(row.GetCell(headers["Format"])),
                Genre = GetStringValue(row.GetCell(headers["Genre"])),
                Tags = matchedTags
            });
        }

        return items;
    }

    private List<Sale> GetSales(HSSFWorkbook workbook, List<Item> items)
    {
        var sheet = workbook.GetSheet("Sales")
                    ?? throw new InvalidOperationException("Could not find .xls sheet with the name 'Sales'");
        var headers = GetHeaders(sheet.GetRow(0));
        var sales = new List<Sale>();

        for (var i = 1; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row is null) continue;

            var itemEntries = GetStringValue(row.GetCell(headers["Sale"]))
                .Split(',')
                .Select(entry => entry.Trim().Split(':'))
                .ToList();

            var matchedItems = itemEntries.Select(parts =>
            {
                var name = parts[0].Trim();
                var quantity = int.Parse(parts[1].Trim());
                var item = items.FirstOrDefault(item => item.Name == name);

                return new SaleItem
                {
                    Item = item!,
                    Quantity = quantity
                };
            }).Where(si => si.Item != null!).ToList();
            
            sales.Add(new Sale
            {
                SoldItems = matchedItems
            });
        }

        return sales;
    }
    
    //------- Helper Methods -------
    private Dictionary<string, int> GetHeaders(IRow row)
    {
        return row.Cells
            .ToDictionary(
                c => c.StringCellValue,
                c => c.ColumnIndex);
    }

    private string GetStringValue(ICell cell)
    {
        return cell.CellType switch
        {
            CellType.String => cell.StringCellValue,
            CellType.Numeric => cell.NumericCellValue.ToString(),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            _ => cell.StringCellValue
        };
    }

    private T GetEnumValue<T>(ICell cell)  where T : struct, Enum
    {
        var success = Enum.TryParse<T>(cell.StringCellValue, true, out var value);
        if (!success)
            throw new InvalidOperationException($"Could not parse {cell.StringCellValue} into type {typeof(T).Name}");
        return value;
    }

    private decimal GetDecimalValue(ICell cell)
    {
        return cell.CellType switch
        {
            CellType.Numeric => (decimal)cell.NumericCellValue,
            CellType.String => decimal.Parse(cell.StringCellValue),
            _ => 0
        };
    }
}