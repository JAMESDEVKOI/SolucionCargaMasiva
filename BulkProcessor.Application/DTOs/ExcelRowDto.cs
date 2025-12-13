namespace BulkProcessor.Application.DTOs
{
    public class ExcelRowDto
    {
        public int RowNumber { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string? NombreProducto { get; set; }
        public string? Categoria { get; set; }
        public decimal? Precio { get; set; }
        public int? Stock { get; set; }
        public string? Proveedor { get; set; }
        public string? Descripcion { get; set; }
        public Dictionary<string, object?> RawData { get; set; } = new();
    }
}
