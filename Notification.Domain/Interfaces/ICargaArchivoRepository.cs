using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces
{
    public interface ICargaArchivoRepository
    {
        Task<CargaArchivo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task UpdateAsync(CargaArchivo carga, CancellationToken cancellationToken = default);
    }
}
