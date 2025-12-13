namespace BulkProcessor.Application.DTOs
{
    public record CargaMasivaRequestedDto(
        int IdCarga,
        string FileId,
        string NombreArchivo,
        string Usuario,
        string Periodo
    );
}
