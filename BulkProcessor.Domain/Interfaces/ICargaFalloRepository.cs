using BulkProcessor.Domain.Entities;

namespace BulkProcessor.Domain.Interfaces
{
    public interface ICargaFalloRepository
    {
        Task BulkInsertAsync(IEnumerable<CargaFallo> fallos, CancellationToken cancellationToken = default);
    }
}
