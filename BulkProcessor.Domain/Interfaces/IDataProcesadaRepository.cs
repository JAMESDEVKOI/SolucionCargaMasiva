using BulkProcessor.Domain.Entities;

namespace BulkProcessor.Domain.Interfaces
{
    public interface IDataProcesadaRepository
    {
        Task<bool> ExistsByCodigoProductoAsync(string codigoProducto, CancellationToken cancellationToken = default);
        Task<int> BulkInsertAsync(IEnumerable<DataProcesada> data, CancellationToken cancellationToken = default);
    }
}
