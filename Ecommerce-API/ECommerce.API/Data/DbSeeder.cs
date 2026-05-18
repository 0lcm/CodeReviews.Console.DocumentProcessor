using ECommerce.API.Models;
using ECommerce.API.Models.Import_Models;
using ECommerce.API.Services.Import_Export;
using ECommerce.API.Services.Import_Export.ServiceFactories;
using Microsoft.Extensions.Logging.Configuration;

namespace ECommerce.API.Data;

public static class DbSeeder
{
    public static async Task RunSeederAsync(ApiDbContext db, ImportServiceFactory serviceFactory, ILogger logger,
        SeedingOptions seedOptions)
    {
        try
        {
            await SeedDatabaseAsync(db, serviceFactory, seedOptions);
        }
        catch (MissingSeedDataException ex)
        {
            logger.LogWarning(ex.Message);
            logger.LogWarning("Update your seeding file, or skip seeding with the argument --skip-seeding.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An unexpected error occurred during seeding.");
            Environment.Exit(1);
        }
    }
    
    private static async Task SeedDatabaseAsync(ApiDbContext db, ImportServiceFactory serviceFactory, SeedingOptions seedOptions)
    {
        if (seedOptions.SkipSeeding) return;
        
        var importService = serviceFactory.Create(out var filePaths);
        var seedData = importService.GetSeedData(filePaths);

        List<string> missingDataCategories = [];
        
        if (!seedOptions.SkipTagSeeding && !seedData.Tags.Any())
            missingDataCategories.Add("Tags");

        if (!seedOptions.SkipItemSeeding && !seedData.Items.Any())
            missingDataCategories.Add("Items");

        if (!seedOptions.SkipSaleSeeding && !seedData.Sales.Any())
            missingDataCategories.Add("Sales");
        
        if (missingDataCategories.Any()) throw new MissingSeedDataException(missingDataCategories);

        if (!seedOptions.SkipTagSeeding && seedData.Tags.Any())
        {
            await db.Tags.AddRangeAsync(seedData.Tags);
            await db.SaveChangesAsync();
        }

        if (!seedOptions.SkipItemSeeding && seedData.Items.Any())
        {
            await db.Items.AddRangeAsync(seedData.Items);
            await db.SaveChangesAsync();
        }

        if (!seedOptions.SkipSaleSeeding && seedData.Sales.Any())
        {
            await db.Sales.AddRangeAsync(seedData.Sales);
            await db.SaveChangesAsync();
        }
    }
}