using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using IxcPosVendaWorker.Models;

namespace IxcPosVendaWorker.Services;

public class IxcApiService : IIxcApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IxcApiService> _logger;

    public IxcApiService(HttpClient httpClient, IConfiguration configuration, ILogger<IxcApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = _configuration["IxcApi:BaseUrl"];
        var tokenId = _configuration["IxcApi:TokenId"];
        var tokenHash = _configuration["IxcApi:TokenHash"];

        _httpClient.BaseAddress = new Uri(baseUrl!);

        // Configura Basic Auth com o token IXC (ID:HASH)
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{tokenId}:{tokenHash}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<List<ContratoDto>> GetContratosAtivadosHojeAsync()
    {
        var hoje = DateTime.Now.ToString("yyyy-MM-dd");

        var body = new
        {
            qtype = "cliente_contrato.data_ativacao",
            query = hoje,
            oper = "=",
            page = "1",
            rp = "100",
            sortname = "cliente_contrato.id",
            sortorder = "desc"
        };

        _logger.LogInformation("🟢 Consultando IXC para contratos ativados em: {Data}", hoje);

        var response = await PostAsync<ContratoDto>("cliente_contrato", body);

        _logger.LogInformation("🟢 API retornou {Count} contratos novos para hoje.", response.Count);

        return response;
    }

    public async Task<List<ContratoDto>> GetContratosCanceladosHojeAsync()
    {
        var hoje = DateTime.Now.ToString("yyyy-MM-dd");

        var body = new
        {
            qtype = "cliente_contrato.ultima_atualizacao",
            query = hoje,
            oper = "=",
            page = "1",
            rp = "100",
            sortname = "cliente_contrato.id",
            sortorder = "desc",
            pesquisa = new[]
            {
            new
            {
                TB = "cliente_contrato",
                CAMPO = "status",
                OPER = "=",
                VALOR = "D"
            },
            new
            {
                TB = "cliente_contrato",
                CAMPO = "ultima_atualizacao",
                OPER = "like",
                VALOR = hoje
            }
        }
        };

        _logger.LogInformation("🔴 Consultando contratos desativados em: {Data}", hoje);

        var response = await PostAsync<ContratoDto>("cliente_contrato", body);

        _logger.LogInformation("🔴 {Count} contratos desativados encontrados hoje.", response.Count);

        return response;
    }

    private async Task<List<T>> PostAsync<T>(string endpoint, object body)
    {
        var jsonBody = JsonSerializer.Serialize(body);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };

        // Header obrigatório do IXC
        request.Headers.Add("ixcsoft", "listar");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return apiResponse?.Registros ?? new List<T>();
    }

    public async Task<ClienteDto> GetClienteAsync(string idCliente)
    {
        try
        {
            var body = new
            {
                qtype = "cliente.id",
                query = idCliente,
                oper = "=",
                page = "1",
                rp = "1"
            };

            var response = await IxcPostAsync("cliente", body);

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var cliente = apiResponse?.Registros?.FirstOrDefault();
            return cliente ?? throw new Exception($"Cliente {idCliente} não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔴 Erro ao buscar cliente {ClienteId}", idCliente);
            throw;
        }
    }

    private async Task<string> IxcPostAsync(string relativeUrl, object body)
    {
        var json = JsonSerializer.Serialize(body);

        using var req = new HttpRequestMessage(HttpMethod.Post, relativeUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        // Header obrigatório do IXC para consultas
        req.Headers.Add("ixcsoft", "listar");

        using var resp = await _httpClient.SendAsync(req);

        if (!resp.IsSuccessStatusCode)
        {
            var errorContent = await resp.Content.ReadAsStringAsync();
            _logger.LogError("❌ Erro na API IXC: {StatusCode} - {Content}", resp.StatusCode, errorContent);
            resp.EnsureSuccessStatusCode();
        }

        return await resp.Content.ReadAsStringAsync();
    }
}