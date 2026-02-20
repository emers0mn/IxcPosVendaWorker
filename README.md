# Pós-Venda - Envio de Email de Boas-Vindas Personalizado

## Descrição
Sistema automatizado para análise e envio de emails personalizados de boas-vindas para clientes.  
Como não há integração direta com fonte de dados, o sistema executa consultas periódicas a cada 10 minutos para capturar novos clientes e disparar os emails correspondentes.

## Tecnologias
- **.NET 8** (ou versão específica)
- **C#**
- **Entity Framework Core** (se aplicável)
- **SQLite/Outro Banco** (se aplicável)
- **MailKit/SmtpClient** para envio de emails

## Instalação e Configuração

### Pré-requisitos
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- SQLite (ou outro banco configurado)

### Passos
```bash
# Clone o repositório
git clone https://github.com/seu-usuario/pos-venda-emails.git
cd pos-venda-emails

# Restaure as dependências
dotnet restore

# Configure as variáveis de ambiente
cp appsettings.Example.json appsettings.Development.json
# Edite o arquivo com suas configurações

# Execute o projeto
dotnet run
```

## Docker

### Linux
```bash
# Rode um publish para linux
dotnet publish -c Release -r linux-x64 -o ./publish-linux

# Rode o Dockerfile
docker build -t posvenda-worker .

# Rodar com docker
docker run -d --name posvenda --restart always posvenda-worker

```