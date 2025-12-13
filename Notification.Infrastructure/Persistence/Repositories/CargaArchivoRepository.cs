using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Persistence.Repositories
{
    public class CargaArchivoRepository : ICargaArchivoRepository
    {
        private readonly NotificationDbContext _context;

        public CargaArchivoRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<CargaArchivo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.CargasArchivos
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public Task UpdateAsync(CargaArchivo carga, CancellationToken cancellationToken = default)
        {
            _context.CargasArchivos.Update(carga);
            return Task.CompletedTask;
        }
    }
}
