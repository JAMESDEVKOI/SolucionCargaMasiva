# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Notification.Worker/Notification.Worker.csproj", "Notification.Worker/"]
COPY ["Notification.Application/Notification.Application.csproj", "Notification.Application/"]
COPY ["Notification.Infrastructure/Notification.Infrastructure.csproj", "Notification.Infrastructure/"]
COPY ["Notification.Domain/Notification.Domain.csproj", "Notification.Domain/"]

# Restaurar dependencias
RUN dotnet restore "Notification.Worker/Notification.Worker.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar
WORKDIR "/src/Notification.Worker"
RUN dotnet build "Notification.Worker.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Notification.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Worker.dll"]
