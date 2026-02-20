using System.Text.Json.Serialization;

namespace IxcPosVendaWorker.Models;

public class ClienteDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("razao")]
    public string Razao { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("fantasia")]
    public string? Fantasia { get; set; }

    [JsonPropertyName("ativo")]
    public string Ativo { get; set; } = string.Empty;
}