# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["FileControl.API/FileControl.API.csproj", "FileControl.API/"]
COPY ["FileControl.Application/FileControl.Application.csproj", "FileControl.Application/"]
COPY ["FileControl.Domain/FileControl.Domain.csproj", "FileControl.Domain/"]
COPY ["FileControl.Infrastructure/FileControl.Infrastructure.csproj", "FileControl.Infrastructure/"]

RUN dotnet restore "./FileControl.API/FileControl.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/FileControl.API"
RUN dotnet build "./FileControl.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "./FileControl.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileControl.API.dll"]
