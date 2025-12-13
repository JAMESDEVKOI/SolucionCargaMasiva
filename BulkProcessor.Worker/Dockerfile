# BulkProcessor Worker Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar todos los proyectos
COPY ["BulkProcessor.Worker/BulkProcessor.Worker.csproj", "BulkProcessor.Worker/"]
COPY ["BulkProcessor.Application/BulkProcessor.Application.csproj", "BulkProcessor.Application/"]
COPY ["BulkProcessor.Domain/BulkProcessor.Domain.csproj", "BulkProcessor.Domain/"]
COPY ["BulkProcessor.Infrastructure/BulkProcessor.Infrastructure.csproj", "BulkProcessor.Infrastructure/"]

# Restore
RUN dotnet restore "BulkProcessor.Worker/BulkProcessor.Worker.csproj"

# Copiar todo el código fuente
COPY . .

# Build
WORKDIR "/src/BulkProcessor.Worker"
RUN dotnet build "BulkProcessor.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BulkProcessor.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BulkProcessor.Worker.dll"]