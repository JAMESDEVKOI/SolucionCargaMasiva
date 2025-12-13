namespace BulkProcessor.Domain.Entities
{
    public class CargaFallo
    {
        public int Id { get; set; }
        public int IdCarga { get; set; }
        public int RowNumber { get; set; }
        public string? CodigoProducto { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? RawData { get; set; }
        public DateTime CreatedAt { get; set; }
        public CargaArchivo? CargaArchivo { get; set; }
    }
}
