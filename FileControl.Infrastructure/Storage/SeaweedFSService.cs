using FileControl.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FileControl.Infrastructure.Storage
{
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

        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Solicitar asignación de fileId al Master
                var assignUrl = $"{_settings.MasterUrl}/dir/assign";
                var assignResponse = await _httpClient.GetAsync(assignUrl, cancellationToken);
                Console.WriteLine("11112121");
                assignResponse.EnsureSuccessStatusCode();

                var assignContent = await assignResponse.Content.ReadAsStringAsync(cancellationToken);
                
                var assignData = JsonSerializer.Deserialize<AssignResponse>(assignContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (assignData == null || string.IsNullOrEmpty(assignData.Fid))
                {
                    throw new InvalidOperationException("No se pudo obtener un fileId de SeaweedFS");
                }

                // 2. Subir el archivo al servidor asignado
                var uploadUrl = $"http://{assignData.Url}/{assignData.Fid}";

                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                content.Add(streamContent, "file", fileName);

                var uploadResponse = await _httpClient.PostAsync(uploadUrl, content, cancellationToken);
                uploadResponse.EnsureSuccessStatusCode();

                _logger.LogInformation(
                    "Archivo {FileName} subido exitosamente a SeaweedFS con FID: {Fid}",
                    fileName, assignData.Fid);

                return assignData.Fid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir archivo {FileName} a SeaweedFS", fileName);
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(
            string fileId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var fid = NormalizeFid(fileId);

                var volumeId = ExtractVolumeId(fid);
                var lookupUrl = $"{_settings.MasterUrl}/dir/lookup?volumeId={Uri.EscapeDataString(volumeId)}";

                var lookupResponse = await _httpClient.GetAsync(lookupUrl, cancellationToken);
                lookupResponse.EnsureSuccessStatusCode();

                var lookupContent = await lookupResponse.Content.ReadAsStringAsync(cancellationToken);
                var lookupData = JsonSerializer.Deserialize<LookupResponse>(lookupContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (lookupData?.Locations == null || !lookupData.Locations.Any())
                    throw new InvalidOperationException($"No se encontró ubicación para el archivo (FID: {fid}, volumeId: {volumeId})");

                var volumeUrl = lookupData.Locations.First().Url;
                var downloadUrl = $"http://{volumeUrl}/{fid}";

                var response = await _httpClient.GetAsync(downloadUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar archivo con FID: {FileId}", fileId);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(
            string fileId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var fid = NormalizeFid(fileId);
                var volumeId = ExtractVolumeId(fid);

                var lookupUrl = $"{_settings.MasterUrl}/dir/lookup?volumeId={Uri.EscapeDataString(volumeId)}";
                var lookupResponse = await _httpClient.GetAsync(lookupUrl, cancellationToken);
                lookupResponse.EnsureSuccessStatusCode();

                var lookupContent = await lookupResponse.Content.ReadAsStringAsync(cancellationToken);
                var lookupData = JsonSerializer.Deserialize<LookupResponse>(lookupContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (lookupData?.Locations == null || !lookupData.Locations.Any())
                    return false;

                var deleteUrl = $"http://{lookupData.Locations.First().Url}/{fid}";
                var response = await _httpClient.DeleteAsync(deleteUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Archivo con FID: {FileId} eliminado exitosamente", fid);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivo con FID: {FileId}", fileId);
                return false;
            }
        }
        private static string NormalizeFid(string fileId)
        {            
            return (fileId ?? string.Empty).Trim().Trim('"');
        }

        private static string ExtractVolumeId(string fileId)
        {
            var fid = NormalizeFid(fileId);
            var parts = fid.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length > 0 ? parts[0] : fid;
        }

        private class AssignResponse
        {
            public string Fid { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string PublicUrl { get; set; } = string.Empty;
        }

        private class LookupResponse
        {
            public string VolumeId { get; set; } = string.Empty;
            public List<Location> Locations { get; set; } = new();
        }

        private class Location
        {
            public string Url { get; set; } = string.Empty;
            public string PublicUrl { get; set; } = string.Empty;
        }
    }
}
