namespace FileControl.Application.DTOs
{
    public record CargaArchivoListDto(
        int Id,
        string NombreArchivo,
        string Usuario,
        string Periodo,
        string Estado,
        DateTime FechaRegistro,
        DateTime? FechaInicioProceso,
        DateTime? FechaFinProceso,
        int TotalRegistros,
        int RegistrosProcesados,
        int RegistrosExitosos,
        int RegistrosFallidos,
        string? MensajeError
    );
}
