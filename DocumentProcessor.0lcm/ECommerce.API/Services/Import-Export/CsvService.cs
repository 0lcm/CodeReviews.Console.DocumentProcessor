using System.Globalization;
using System.IO.Compression;
using CsvHelper;
using ECommerce.API.Data;
using ECommerce.API.Interfaces.Import_Export;
using ECommerce.API.Models;
using ECommerce.API.Models.Import_Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Services.Import_Export;

public class CsvService : IImportService, IExportService
{
    public SeedData GetSeedData(IEnumerable<string> filePaths)
    {
        var filePathList = filePaths.ToList();
        
        var tags = GetTags(filePathList);
        var items = GetItems(filePathList, tags);
        var sales = GetSales(filePathList, items);

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

        var zipStream = new MemoryStream();
        await using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            await ExportSales(sales, archive);
            await ExportItems(items, archive);
            await ExportTags(tags, archive);   
        }

        zipStream.Position = 0;
        return zipStream;
    }

    //------- Import Methods -------
    private static List<Tag> GetTags(List<string> filePaths)
    {
        var filePath = filePaths.First(f => Path.GetFileNameWithoutExtension(f).ToLower() == "tags");
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<TagCsvRecord>().ToList();
        
        return records.Select(record =>
        {
            return new Tag
            {
                TagName = record.Name
            };
        }).ToList();
    }

    private static List<Item> GetItems(List<string> filePaths, List<Tag> tags)
    {
        var filePath = filePaths.First(f => Path.GetFileNameWithoutExtension(f).ToLower() == "items");
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<ItemCsvRecord>().ToList();
        
        return records.Select(record =>
        {
            var tagNames = record.Tags.Split(',').Select(t => t.Trim()).ToList();
            var matchedTags = tags.Where(t => tagNames.Contains(t.TagName)).ToList();

            return new Item
            {
                Name = record.Name,
                Artist = record.Artist,
                Format = record.Format,
                Type = record.Type,
                Genre = record.Genre,
                Price = record.Price,
                Tags = matchedTags
            };
        }).ToList();
    }

    private static List<Sale> GetSales(List<string> filePaths, List<Item> items)
    {
        var filePath = filePaths.First(f => Path.GetFileNameWithoutExtension(f).ToLower() == "sales");
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<SaleCsvRecord>().ToList();

        return records.Select(record =>
        {
            var itemEntries = record.SoldItems
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
    private async Task ExportSales(IEnumerable<Sale> sales, ZipArchive archive)
    {
        var rows = sales.Select(s => new
        {
            Id = s.SaleId,
            SoldItems = string.Join(", ", s.SoldItems.Select(si => $"{si.Item.Name} x {si.Quantity}")),
            Price = s.TotalPrice
        });
        
        await WriteCsvEntry(archive, rows, "sales.csv");
    }

    private async Task ExportItems(IEnumerable<Item> items, ZipArchive archive)
    {
        var rows = items.Select(i => new 
        {
            i.Artist,
            i.Name,
            i.Type,
            i.Format,
            i.Genre,
            i.Price,
            Tags = string.Join(",", i.Tags.Select(tag => tag.TagName)),
        });
        
        await WriteCsvEntry(archive, rows, "items.csv");
    }

    private async Task ExportTags(IEnumerable<Tag> tags, ZipArchive archive)
    {
        var rows = tags.Select(t => new
        {
            t.TagId,
            t.TagName,
        });
        
        await WriteCsvEntry(archive, rows, "tags.csv");
    }

    private async Task WriteCsvEntry<T>(ZipArchive archive, IEnumerable<T> rows, string fileName)
    {
        var entry = archive.CreateEntry(fileName);
        await using var stream = await entry.OpenAsync();
        await using var writer = new StreamWriter(stream, leaveOpen: true);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        await csv.WriteRecordsAsync(rows);
    }
}