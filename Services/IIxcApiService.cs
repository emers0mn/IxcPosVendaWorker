using IxcPosVendaWorker.Models;

namespace IxcPosVendaWorker.Services;

public interface IIxcApiService
{
    Task<List<ContratoDto>> GetContratosAtivadosHojeAsync();
    Task<List<ContratoDto>> GetContratosCanceladosHojeAsync();
    Task<ClienteDto> GetClienteAsync(string idCliente);
}