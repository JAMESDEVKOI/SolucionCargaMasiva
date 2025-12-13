namespace FileControl.Application.DTOs
{
    public record CargaMasivaMessageDto(
        int IdCarga,
        string FileId,
        string NombreArchivo,
        string Usuario,
        string Periodo
    );
}
