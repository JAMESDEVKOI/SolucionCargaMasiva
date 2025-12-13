using BulkProcessor.Domain.Entities;
using BulkProcessor.Domain.Enums;

namespace BulkProcessor.Domain.Interfaces
{
    public interface ICargaArchivoRepository
    {
        Task<CargaArchivo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        //Task<CargaArchivo?> GetByPeriodoAndEstadoAsync(string periodo, CargaEstado[] estados, int excludeId, CancellationToken cancellationToken = default);
        Task UpdateAsync(CargaArchivo cargaArchivo, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPeriodoAndEstadoAsync(string periodo, CargaEstado[] estados, int? excludeId, CancellationToken cancellationToken = default);
    }
}
