using FileControl.Domain.Entities;

namespace FileControl.Domain.Interfaces
{
    public interface ICargaArchivoRepository
    {
        Task<CargaArchivo> CreateAsync(CargaArchivo cargaArchivo, CancellationToken cancellationToken = default);
        Task<CargaArchivo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(CargaArchivo cargaArchivo, CancellationToken cancellationToken = default);
        Task<IEnumerable<CargaArchivo>> GetByUsuarioAsync(string usuario, CancellationToken cancellationToken = default);
        Task<IEnumerable<CargaArchivo>> GetByPeriodoAsync(string periodo, CancellationToken cancellationToken = default);
    }
}
