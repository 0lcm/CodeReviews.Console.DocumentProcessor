using ECommerce.API.Models;

namespace ECommerce.API.Interfaces.Import_Export;

public interface IImportService
{
    public SeedData GetSeedData(IEnumerable<string> filePaths);
}