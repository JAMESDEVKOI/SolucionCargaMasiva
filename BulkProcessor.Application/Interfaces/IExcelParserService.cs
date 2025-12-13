using BulkProcessor.Application.DTOs;

namespace BulkProcessor.Application.Interfaces
{
    public interface IExcelParserService
    {
        Task<IEnumerable<ExcelRowDto>> ParseExcelAsync(Stream excelStream, CancellationToken cancellationToken = default);
    }
}
