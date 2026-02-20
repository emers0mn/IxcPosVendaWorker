namespace IxcPosVendaWorker.Services;

public interface IEmailService
{
    Task EnviarEmailPosVendaAsync(string emailDestino, string nomeCliente, string nomePlano);
}