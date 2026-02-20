using IxcPosVendaWorker.Data;
using IxcPosVendaWorker.Services;
using Microsoft.EntityFrameworkCore;

namespace IxcPosVendaWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
    }

    private TimeSpan CalcularProximoIntervalo(TimeSpan horaInicio, TimeSpan horaFim, TimeSpan intervaloNormal)
    {
        var agora = DateTime.Now;
        var horaAtual = agora.TimeOfDay;

        var inicioHoje = agora.Date.Add(horaInicio);
        var fimHoje = agora.Date.Add(horaFim);

        // Caso 1: Estamos DENTRO da janela (13:00 - 14:00)
        if (horaAtual >= horaInicio && horaAtual < horaFim)
        {
            _logger.LogInformation("🟢 Dentro do horário de operação. Próxima execução em {Intervalo} minutos.",
                intervaloNormal.TotalMinutes);
            return intervaloNormal;
        }

        // Caso 2: Estamos ANTES da janela (ex: 12:30)
        if (horaAtual < horaInicio)
        {
            var tempoAteInicio = inicioHoje - agora;
            _logger.LogInformation("🟣 Fora do horário. Aguardando até {Hora} ({Minutos} minutos).",
                inicioHoje.ToString("HH:mm"),
                tempoAteInicio.TotalMinutes);
            return tempoAteInicio;
        }

        // Caso 3: Estamos DEPOIS da janela (ex: 14:10) - Espera até amanhã 13:00
        var inicioAmanha = agora.Date.AddDays(1).Add(horaInicio);
        var tempoAteAmanha = inicioAmanha - agora;
        _logger.LogInformation("🟣 Horário encerrado. Próxima execução amanhã às {Hora} ({Horas}h {Minutos}m).",
            inicioAmanha.ToString("HH:mm"),
            (int)tempoAteAmanha.TotalHours,
            tempoAteAmanha.Minutes);
        return tempoAteAmanha;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Configurações de janela de horário
        var horaInicio = new TimeSpan(9, 0, 0);  // 13:00
        var horaFim = new TimeSpan(18, 0, 0);     // 14:00
        var intervaloNormal = TimeSpan.FromMinutes(10);

        _logger.LogInformation("🟢 Worker de Pós-Venda iniciado.");
        _logger.LogInformation("🟢 Janela de operação: {Inicio} às {Fim}",
            horaInicio.ToString(@"hh\:mm"),
            horaFim.ToString(@"hh\:mm"));

        while (!stoppingToken.IsCancellationRequested)
        {
            var proximoIntervalo = CalcularProximoIntervalo(horaInicio, horaFim, intervaloNormal);

            // Só processa se estiver dentro da janela
            var horaAtual = DateTime.Now.TimeOfDay;
            if (horaAtual >= horaInicio && horaAtual < horaFim)
            {
                try
                {
                    await ProcessarContratosAtivosAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🟥 Erro crítico no Worker");
                }
            }

            // Dorme pelo tempo calculado
            await Task.Delay(proximoIntervalo, stoppingToken);
        }
    }
    private async Task ProcessarContratosAtivosAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var ixcService = scope.ServiceProvider.GetRequiredService<IIxcApiService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _logger.LogInformation("🔍 Buscando contratos ativados hoje no IXC...");

        try
        {
            var contratos = await ixcService.GetContratosAtivadosHojeAsync();
            var contratosCancelados = await ixcService.GetContratosCanceladosHojeAsync();

            _logger.LogInformation("🟩 Encontrados {Count} contratos ativados hoje", contratos.Count);
            _logger.LogInformation("🟥 Cancelados hoje {Count}", contratosCancelados.Count);

            foreach (var contrato in contratos)
            {
                // Verifica se já processamos este contrato
                var jaProcessado = await dbContext.ContratosProcessados
                    .AnyAsync(c => c.IdContrato == contrato.Id);

                if (jaProcessado)
                {
                    _logger.LogDebug("🟪  Contrato {Id} já foi processado anteriormente", contrato.Id);
                    continue;
                }

                try
                {
                    _logger.LogInformation("🟩 Processando contrato {Id} (Cliente: {ClienteId})...",
                        contrato.Id, contrato.IdCliente);

                    // Busca dados do cliente
                    var cliente = await ixcService.GetClienteAsync(contrato.IdCliente);

                    // Valida email
                    if (string.IsNullOrWhiteSpace(cliente.Email))
                    {
                        _logger.LogWarning("🟨  Contrato {Id}: Cliente {ClienteId} sem email cadastrado",
                            contrato.Id, contrato.IdCliente);

                        // Marca como processado mesmo sem email para não ficar tentando
                        dbContext.ContratosProcessados.Add(new ContratoProcessado
                        {
                            IdContrato = contrato.Id,
                            DataProcessamento = DateTime.UtcNow,
                            EmailEnviado = false,
                            Observacao = "Cliente sem email cadastrado"
                        });
                        await dbContext.SaveChangesAsync();
                        continue;
                    }

                    // Envia email de pós-venda
                    await emailService.EnviarEmailPosVendaAsync(
                        cliente.Email,
                        cliente.Razao,
                        contrato.Contrato
                    );

                    // Marca como processado
                    dbContext.ContratosProcessados.Add(new ContratoProcessado
                    {
                        IdContrato = contrato.Id,
                        DataProcessamento = DateTime.UtcNow,
                        EmailEnviado = true,
                        EmailDestino = cliente.Email
                    });
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation("🟩 Email enviado para {Email} - Plano: {Plano}",
                        cliente.Email, contrato.Contrato);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🟥 Erro ao processar contrato {Id}", contrato.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🟥 Erro ao buscar contratos no IXC");
        }
    }
}