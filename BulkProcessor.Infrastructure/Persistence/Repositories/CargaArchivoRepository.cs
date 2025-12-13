using BulkProcessor.Domain.Entities;
using BulkProcessor.Domain.Enums;
using BulkProcessor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BulkProcessor.Infrastructure.Persistence.Repositories
{
    public class CargaArchivoRepository : ICargaArchivoRepository
    {
        private readonly BulkProcessorDbContext _context;

        public CargaArchivoRepository(BulkProcessorDbContext context)
        {
            _context = context;
        }

        public async Task<CargaArchivo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.CargasArchivo
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByPeriodoAndEstadoAsync(
            string periodo,
            CargaEstado[] estados,
            int? excludeId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.CargasArchivo
                .Where(c => c.Periodo == periodo && estados.Contains(c.Estado));

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public Task UpdateAsync(CargaArchivo carga, CancellationToken cancellationToken = default)
        {
            _context.CargasArchivo.Update(carga);
            return Task.CompletedTask;
        }
    }
}
