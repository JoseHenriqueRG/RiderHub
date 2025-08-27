# Build da aplicação usando SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia os arquivos .csproj e restaura dependências
COPY ["RiderHub.WebApi/RiderHub.WebApi.csproj", "RiderHub.WebApi/"]
COPY ["RiderHub.Application/RiderHub.Application.csproj", "RiderHub.Application/"]
COPY ["RiderHub.Domain/RiderHub.Domain.csproj", "RiderHub.Domain/"]
COPY ["RiderHub.Infrastructure/RiderHub.Infrastructure.csproj", "RiderHub.Infrastructure/"]
RUN dotnet restore "RiderHub.WebApi/RiderHub.WebApi.csproj"

# Copia todo o projeto e build
COPY . .
WORKDIR /app/RiderHub.WebApi
RUN dotnet build "RiderHub.WebApi.csproj" -c Release -o /app/build

# Publica a aplicação
FROM build AS publish
RUN dotnet publish "RiderHub.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime final usando aspnet:8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia a aplicação publicada
COPY --from=publish /app/publish .

# Expõe a porta da API
EXPOSE 8080

# Entry point para rodar a API
ENTRYPOINT ["dotnet", "RiderHub.WebApi.dll"]
