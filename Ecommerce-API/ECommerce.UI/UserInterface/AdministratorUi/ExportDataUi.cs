using System.Net;
using ECommerce.UI.Helpers;
using ECommerce.UI.Interfaces;
using static ECommerce.UI.Helpers.DisplayHelper;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace ECommerce.UI.UserInterface.AdministratorUi;

internal class ExportDataUi(IConfiguration configuration, IExportService exportService)
{
    private readonly string _exportFolderPath = Environment.ExpandEnvironmentVariables(
        configuration["Exporting:ExportFolderPath"] ??
        throw new InvalidOperationException("Could not parse Exporting:ExportFolderPath from appSettings.json"));

    private readonly string _exportFileType = configuration["Exporting:ExportFileType"] ??
                                              throw new InvalidOperationException(
                                                  "Could not parse Exporting:ExportFileType from appSettings.json");

    internal async Task ExportMenu()
    {
        DisplaySuccess($"Exporting to {_exportFolderPath} as .{_exportFileType}");
        if (!await AnsiConsole.ConfirmAsync("Would you like to export data for sales, items, and tags locally?")) return;

        try
        {
            await exportService.ExportDataAsync();
            DisplaySuccess($"Successfully exported data to {_exportFolderPath}");
            UiHelper.WaitForUser();
        }
        catch (NotSupportedException ex)
        {
            DisplayWarning($"An exception has occurred: {ex.Message}");
            DisplayWarning(
                "Please make sure you are using a pdf, xlsx, or csv file type in your appSettings.json file, and ensure you are not including a '.' in the file type.");
            UiHelper.WaitForUser();
        }
        catch (Exception ex)
        {
            UiHelper.DisplayCaughtException(ex);
        }
    }
}