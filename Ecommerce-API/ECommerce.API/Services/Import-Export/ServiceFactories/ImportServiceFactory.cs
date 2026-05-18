using ECommerce.API.Interfaces.Import_Export;

namespace ECommerce.API.Services.Import_Export.ServiceFactories;

public class ImportServiceFactory(IConfiguration configuration, IServiceProvider provider)
{
    private readonly string _folderPath = Environment.ExpandEnvironmentVariables(
        configuration["Seeding:FolderPath"] ?? throw new InvalidOperationException(
            "Could not parse Exporting:ExportFilePath in appSettings.json"));

    public IImportService Create(out IEnumerable<string> filePaths)
    {
        var xlsxFiles = Directory.GetFiles(_folderPath, "*.xlsx");
        var csvFiles = Directory.GetFiles(_folderPath, "*.csv");
        var xlsFiles = Directory.GetFiles(_folderPath, "*.xls");

        if (xlsxFiles.Length > 0)
        {
            filePaths = xlsxFiles;
            return provider.GetRequiredService<XlsxService>();
        }

        if (csvFiles.Length > 0)
        {
            filePaths =  csvFiles;
            return provider.GetRequiredService<CsvService>();
        }

        if (xlsFiles.Length > 0)
        {
            filePaths = xlsFiles;
            return provider.GetRequiredService<XlsService>();
        }
        
        throw new FileNotFoundException($"Could not find any supported files for folder {_folderPath}");
    }
}