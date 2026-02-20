using System.Text.Json.Serialization;

namespace IxcPosVendaWorker.Models;

public class ContratoDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("id_cliente")]
    public string IdCliente { get; set; } = string.Empty;

    [JsonPropertyName("id_filial")]
    public string IdFilial { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("status_internet")]
    public string? StatusInternet { get; set; }

    [JsonPropertyName("data_ativacao")]
    public string DataAtivacao { get; set; } = string.Empty;

    [JsonPropertyName("contrato")]
    public string Contrato { get; set; } = string.Empty;

    [JsonPropertyName("id_vd_contrato")]
    public string? IdVdContrato { get; set; }
}