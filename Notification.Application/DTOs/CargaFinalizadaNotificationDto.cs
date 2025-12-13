namespace Notification.Application.DTOs
{
    public record CargaFinalizadaNotificationDto(
        int IdCarga,
        string Usuario,
        DateTime FechaFin,
        string Estado,
        int? Procesados = null,
        int? Insertados = null,
        int? Fallidos = null,
        string? Motivo = null
    );
}
