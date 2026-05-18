using CsvHelper.Configuration.Attributes;
using ECommerce.Shared;

namespace ECommerce.API.Models.Import_Models;

public class TagCsvRecord
{
    public required string Name { get; set; }
}

public class ItemCsvRecord
{
    public required ItemFormat Format { get; set; }
    public required ItemType Type { get; set; }
    public required string Name { get; set; }
    public required string Artist { get; set; }
    public required decimal Price { get; set; }
    public required string Genre { get; set; }
    public string Tags { get; set; } 
}

public class SaleCsvRecord
{ 
    [Name("Sale")]
    public required string SoldItems { get; set; } 
}