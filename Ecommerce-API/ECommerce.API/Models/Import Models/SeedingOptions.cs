namespace ECommerce.API.Models.Import_Models;

public class SeedingOptions
{
    public bool SkipSeeding { get; set; }
    public bool SkipItemSeeding { get; set; }
    public bool SkipTagSeeding { get; set; }
    public bool SkipSaleSeeding { get; set; }
}