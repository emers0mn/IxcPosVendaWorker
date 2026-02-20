using IxcPosVendaWorker.Helpers;
using MailKit.Net.Smtp;
using MimeKit;

namespace IxcPosVendaWorker.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task EnviarEmailPosVendaAsync(string emailDestino, string nomeCliente, string nomePlano)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Smtp:FromName"],
                _configuration["Smtp:FromEmail"]
            ));
            message.To.Add(new MailboxAddress(nomeCliente, _configuration["Smtp:FromEmailTeste"]));
            message.Subject = "Bem-vindo à HAYP!";

            string templatePath = EmailTemplateSelector.ObterCaminhoPorPlano(nomePlano);
            string emailHtml = File.ReadAllText(templatePath);

            emailHtml = emailHtml.Replace("{{NomeCliente}}", nomeCliente);

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = emailHtml
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["Smtp:Host"],
                _configuration.GetValue<int>("Smtp:Port"),
                MailKit.Security.SecureSocketOptions.SslOnConnect
            );

            await client.AuthenticateAsync(
                _configuration["Smtp:Username"],
                _configuration["Smtp:Password"]
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("🟢 Email enviado com sucesso para {Email}", emailDestino);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔴 Erro ao enviar email para {Email}", emailDestino);
            throw;
        }
    }
}