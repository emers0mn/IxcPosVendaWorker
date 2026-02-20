# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["IxcPosVendaWorker.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -r linux-x64 -o /app/publish --no-restore

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IxcPosVendaWorker.dll"]