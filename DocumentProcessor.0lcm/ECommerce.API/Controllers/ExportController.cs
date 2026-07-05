using ECommerce.API.Data;
using ECommerce.API.Interfaces.Import_Export;
using ECommerce.API.Services.Import_Export.ServiceFactories;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController(ExportServiceFactory factory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ExportData([FromQuery] string fileType)
    {
        try
        { 
            var service = factory.Create(fileType, out var db);
            var stream = await service.ExportDataAsync(db);

            return fileType.ToLower().Trim() switch
            {
                "pdf" => File(stream, "application/pdf", "export-data.pdf"),
                "csv" => File(stream, "application/zip", "export-data.zip"),
                "xlsx" => File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "export-data.xlsx"),
                _ => BadRequest($"File type {fileType} is not supported. Please use pdf, csv, or xlsx file types.")
            };
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}