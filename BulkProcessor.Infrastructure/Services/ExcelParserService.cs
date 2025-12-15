using BulkProcessor.Application.DTOs;
using BulkProcessor.Application.Interfaces;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace BulkProcessor.Infrastructure.Services
{
    public class ExcelParserService : IExcelParserService
    {
        private readonly ILogger<ExcelParserService> _logger;

        public ExcelParserService(ILogger<ExcelParserService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ExcelRowDto>> ParseExcelAsync(
            Stream excelStream,
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var rows = new List<ExcelRowDto>();

                try
                {
                    using var workbook = new XLWorkbook(excelStream);
                    var worksheet = workbook.Worksheet(1);

                    var range = worksheet.RangeUsed();

                    if (range == null)
                    {
                        _logger.LogWarning("El archivo Excel está vacío");
                        return rows;
                    }

                    var firstRowUsed = range.FirstRow().RowNumber();
                    var lastRowUsed = range.LastRow().RowNumber();

                    _logger.LogInformation(
                        "Procesando Excel. Filas: {FirstRow} a {LastRow}",
                        firstRowUsed, lastRowUsed);

                    for (int rowNumber = firstRowUsed + 1; rowNumber <= lastRowUsed; rowNumber++)
                    {
                        var row = worksheet.Row(rowNumber);

                        if (row.IsEmpty())
                        {
                            _logger.LogDebug("Fila {RowNumber} está vacía, ignorando", rowNumber);
                            continue;
                        }

                        // Columna 1: Periodo (se ignora, ya viene en el mensaje)
                        var codigoProducto = GetCellValue(row, 2);
                        var nombreProducto = GetCellValue(row, 3);
                        var precioStr = GetCellValue(row, 4);
                        var categoria = GetCellValue(row, 5);
                        var stockStr = GetCellValue(row, 6);
                        var proveedor = GetCellValue(row, 7);
                        var descripcion = GetCellValue(row, 8);   

                        nombreProducto = string.IsNullOrWhiteSpace(nombreProducto) ? "Sin nombre" : nombreProducto;
                        categoria = string.IsNullOrWhiteSpace(categoria) ? "General" : categoria;
                        proveedor = string.IsNullOrWhiteSpace(proveedor) ? "Desconocido" : proveedor;
                        
                        decimal? precio = null;
                        if (!string.IsNullOrWhiteSpace(precioStr))
                        {
                            if (decimal.TryParse(precioStr.Replace(",", "."),
                                System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out var precioValue))
                            {
                                precio = precioValue;
                            }
                        }

                        int? stock = null;
                        if (!string.IsNullOrWhiteSpace(stockStr))
                        {
                            if (int.TryParse(stockStr, out var stockValue))
                            {
                                stock = stockValue;
                            }
                        }


                        var rawData = new Dictionary<string, object?>
                        {
                            { "CodigoProducto", codigoProducto },
                            { "NombreProducto", nombreProducto },
                            { "Categoria", categoria },
                            { "Precio", precioStr },
                            { "Stock", stockStr },
                            { "Proveedor", proveedor },
                            { "Descripcion", descripcion }
                        };

                        rows.Add(new ExcelRowDto
                        {
                            RowNumber = rowNumber,
                            CodigoProducto = codigoProducto,
                            NombreProducto = nombreProducto,
                            Categoria = categoria,
                            Precio = precio,
                            Stock = stock,
                            Proveedor = proveedor,
                            Descripcion = descripcion,
                            RawData = rawData
                        });
                    }

                    _logger.LogInformation(
                        "Excel procesado exitosamente. Total de filas válidas: {Count}",
                        rows.Count);

                    return rows;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al parsear archivo Excel");
                    throw new InvalidOperationException("Error al procesar archivo Excel", ex);
                }
            }, cancellationToken);
        }

        private string GetCellValue(IXLRow row, int columnNumber)
        {
            try
            {
                var cell = row.Cell(columnNumber);

                if (cell == null || cell.IsEmpty())
                    return string.Empty;

                var value = cell.Value;

                if (value.IsBlank)
                    return string.Empty;

                if (value.IsNumber)
                    return value.GetNumber().ToString(System.Globalization.CultureInfo.InvariantCulture);

                if (value.IsDateTime)
                    return value.GetDateTime().ToString("yyyy-MM-dd");
                
                if (value.IsBoolean)
                    return value.GetBoolean().ToString();
                
                return value.GetText().Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Error al leer celda en fila {RowNumber}, columna {ColumnNumber}",
                    row.RowNumber(), columnNumber);
                return string.Empty;
            }
        }
    }
}
