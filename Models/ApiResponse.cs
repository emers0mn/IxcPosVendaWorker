using System.Text.Json.Serialization;
using IxcPosVendaWorker.Helpers;

namespace IxcPosVendaWorker.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("page")]
    public string Page { get; set; } = string.Empty;

[JsonPropertyName("total")]
    [JsonConverter(typeof(StringOrIntConverter))]
    public int Total { get; set; }

    [JsonPropertyName("registros")]
    public List<T> Registros { get; set; } = new();
}