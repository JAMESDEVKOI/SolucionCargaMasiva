namespace BulkProcessor.Domain.Entities
{
    public class DataProcesada
    {
        public int Id { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public string CodigoProducto { get; set; } = string.Empty;
        public string? NombreProducto { get; set; }
        public string? Categoria { get; set; }
        public decimal? Precio { get; set; }
        public int? Stock { get; set; }
        public string? Proveedor { get; set; }
        public string? Descripcion { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
