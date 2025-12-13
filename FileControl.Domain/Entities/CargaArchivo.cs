using FileControl.Domain.Enums;

namespace FileControl.Domain.Entities
{
    public class CargaArchivo
    {
        public int Id { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public string? FileId { get; set; }
        public CargaEstado Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaInicioProceso { get; set; }
        public DateTime? FechaFinProceso { get; set; }
        public string? MensajeError { get; set; }
        public int TotalRegistros { get; set; }
        public int RegistrosProcesados { get; set; }
        public int RegistrosExitosos { get; set; }
        public int RegistrosFallidos { get; set; }
    }
}
