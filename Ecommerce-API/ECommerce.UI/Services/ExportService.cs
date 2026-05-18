using System.IO.Compression;
using System.Net;
using ECommerce.UI.Configuration;
using ECommerce.UI.Helpers;
using ECommerce.UI.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ECommerce.UI.Services;

public class ExportService(IApiService api, IConfiguration configuration) : IExportService
{
    private readonly string _exportFolderPath = Environment.ExpandEnvironmentVariables(
        configuration["Exporting:ExportFolderPath"] ??
        throw new InvalidOperationException("Could not parse Exporting:ExportFolderPath from appSettings.json"));
    
    private readonly string _fileType = configuration["Exporting:ExportFileType"] ??
                                        throw new InvalidOperationException(
                                            "Could not parse Exporting:ExportFileType from appSettings.json");
    
    public async Task ExportDataAsync()
    {
        var exportUrl = Utils.FormatQueryWithExportParams(ApiUris.ExportRequestUri, _fileType);

        var response = Array.Empty<byte>();
        try
        {
            response = await api.GetFileAsync(exportUrl);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.BadRequest)
                throw new NotSupportedException($"Unsupported file type: {_fileType}. Please request a pdf, xlsx, or csv file.");
        }

        switch (_fileType.ToLower().Trim())
        {
            case "pdf":
            case "xlsx": 
                await SaveFileAsync(response);
                break;
            
            case "csv":
                await ExtractAndSaveCsvAsync(response);
                break;
            
            default:
                throw new NotSupportedException($"Unsupported file type: {_fileType}. Please request a pdf, xlsx, or csv file.");
        }
    }

    private async Task SaveFileAsync(byte[] bytes)
    {
        var exportPath = $"{_exportFolderPath}/export-data.{_fileType}";
        Directory.CreateDirectory(_exportFolderPath);
        
        await File.WriteAllBytesAsync(exportPath, bytes);
    }

    private async Task ExtractAndSaveCsvAsync(byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        
        Directory.CreateDirectory(_exportFolderPath);
        
        await using var archive = new ZipArchive(stream);
        foreach (var entry in archive.Entries)
        {
            var filePath = Path.Combine(
                _exportFolderPath, entry.Name);
            
            await using var entryStream = await entry.OpenAsync();
            await using var fileStream = File.Create(filePath);
            await entryStream.CopyToAsync(fileStream);
        }
    }
}