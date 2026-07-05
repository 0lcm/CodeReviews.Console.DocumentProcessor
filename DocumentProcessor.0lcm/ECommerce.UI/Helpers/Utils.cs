using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommerce.Shared.Models;

namespace ECommerce.UI.Helpers;

internal static class Utils
{
    internal static string FormatQueryWithPaginationParams(string baseUrl, int pageNumber, int pageSize,
        string? searchTerm, string? searchGenre, List<TagDto>? searchTags)
    {
        var sb = new StringBuilder();
        sb.Append($"{baseUrl}?PageNumber={pageNumber}&PageSize={pageSize}");

        if (!string.IsNullOrWhiteSpace(searchTerm))
            sb.Append($"&SearchTerm={Uri.EscapeDataString(searchTerm)}");
        if (!string.IsNullOrWhiteSpace(searchGenre))
            sb.Append($"&Genre={Uri.EscapeDataString(searchGenre)}");
        if (searchTags != null && searchTags.Any())
        {
            for (var i = 0; i < searchTags.Count; i++)
            {
                sb.Append($"&SearchTags={searchTags[i].TagName}");
            }
        }

        return sb.ToString();
    }

    internal static string FormatQueryWithExportParams(string baseUrl, string fileType)
    {
        var sb = new StringBuilder();
        sb.Append($"{baseUrl}?FileType={fileType}");
        
        return sb.ToString();
    }

    internal static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}