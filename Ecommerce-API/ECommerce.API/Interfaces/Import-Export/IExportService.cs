using ECommerce.API.Data;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Interfaces.Import_Export;

public interface IExportService
{
    public Task<MemoryStream> ExportDataAsync(ApiDbContext db);
}