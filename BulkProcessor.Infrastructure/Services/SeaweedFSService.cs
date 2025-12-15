using BulkProcessor.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BulkProcessor.Infrastructure.Services
{
    public class SeaweedFSSettings
    {
        public string MasterUrl { get; set; } = "http://localhost:9333";
    }

    public class SeaweedFSService : IFileStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly SeaweedFSSettings _settings;
        private readonly ILogger<SeaweedFSService> _logger;

        public SeaweedFSService(
            HttpClient httpClient,
            IOptions<SeaweedFSSettings> settings,
            ILogger<SeaweedFSService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<Stream> DownloadFileAsync(
            string fileId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Descargando archivo desde SeaweedFS. FileId: {FileId}", fileId);

                // Obtener la ubicación del archivo desde el Master
                var volumeId = GetVolumeId(fileId);
                var lookupUrl = $"{_settings.MasterUrl}/dir/lookup?volumeId={volumeId}";

                _logger.LogInformation("Lookup URL: {LookupUrl}, VolumeId extraído: {VolumeId}", lookupUrl, volumeId);

                var lookupResponse = await _httpClient.GetAsync(lookupUrl, cancellationToken);

                if (!lookupResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al buscar ubicación del archivo: {lookupResponse.StatusCode}");
                }

                var lookupJson = await lookupResponse.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogInformation("Respuesta de lookup: {LookupJson}", lookupJson);

                var lookupData = System.Text.Json.JsonSerializer.Deserialize<SeaweedFSLookupResponse>(
                    lookupJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (lookupData?.Locations == null || !lookupData.Locations.Any())
                {
                    _logger.LogError("No se encontraron ubicaciones. lookupData es null: {IsNull}, Locations es null: {LocationsNull}, Locations count: {Count}",
                        lookupData == null,
                        lookupData?.Locations == null,
                        lookupData?.Locations?.Count ?? 0);
                    throw new Exception("No se encontró ubicación para el archivo");
                }

                // Descargar desde el volumen
                var location = lookupData.Locations[0];
                var downloadUrl = $"http://{location.Url}/{fileId}";

                _logger.LogInformation("Descargando desde: {DownloadUrl}", downloadUrl);

                var downloadResponse = await _httpClient.GetAsync(downloadUrl, cancellationToken);

                if (!downloadResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al descargar archivo: {downloadResponse.StatusCode}");
                }

                // Retornar el stream
                var stream = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken);

                _logger.LogInformation("Archivo descargado exitosamente. FileId: {FileId}", fileId);

                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar archivo desde SeaweedFS. FileId: {FileId}", fileId);
                throw;
            }
        }

        private string GetVolumeId(string fileId)
        {
            var parts = fileId.Split(',');
            return parts.Length > 0 ? parts[0] : fileId;
        }

        private class SeaweedFSLookupResponse
        {
            public List<SeaweedFSLocation>? Locations { get; set; }
        }

        private class SeaweedFSLocation
        {
            public string Url { get; set; } = string.Empty;
            public string PublicUrl { get; set; } = string.Empty;
        }
    }
}
