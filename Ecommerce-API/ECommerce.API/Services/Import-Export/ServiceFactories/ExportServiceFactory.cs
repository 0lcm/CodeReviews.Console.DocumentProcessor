using ECommerce.API.Data;
using ECommerce.API.Interfaces.Import_Export;

namespace ECommerce.API.Services.Import_Export.ServiceFactories;

public class ExportServiceFactory(IServiceProvider provider, ApiDbContext context)
{
    public IExportService Create(string fileType, out ApiDbContext db)
    {
        db = context;
        return fileType.ToLower().Trim() switch
        {
            "pdf" => provider.GetRequiredService<PdfService>(),
            "csv" => provider.GetRequiredService<CsvService>(),
            "xlsx" => provider.GetRequiredService<XlsxService>(),
            _ => throw new NotSupportedException(
                "File type is not supported. Supported file types include pdf, csv, or xlsx")
        };
    }
}