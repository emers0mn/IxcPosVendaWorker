using IxcPosVendaWorker;
using IxcPosVendaWorker.Data;
using IxcPosVendaWorker.Services;
using IxcPosVendaWorker.Helpers;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
var basePath = AppDomain.CurrentDomain.BaseDirectory;

// Configuração do banco SQLite
var dbPath = $"Data Source={Path.Combine(basePath, "contratos_processados.db")}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbPath));

var templatesPath = Path.Combine(basePath, "Templates");
EmailTemplateSelector.ConfigurarCaminho(templatesPath);

// Registra os serviços
builder.Services.AddHttpClient<IIxcApiService, IxcApiService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Registra o Worker
builder.Services.AddHostedService<Worker>();
builder.Services.AddWindowsService();

var host = builder.Build();

// Cria o banco de dados se não existir
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

host.Run();