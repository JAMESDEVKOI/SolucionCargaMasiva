using BulkProcessor.Domain.Entities;
using BulkProcessor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BulkProcessor.Infrastructure.Persistence.Repositories
{
    public class CargaFalloRepository : ICargaFalloRepository
    {
        private readonly BulkProcessorDbContext _context;

        public CargaFalloRepository(BulkProcessorDbContext context)
        {
            _context = context;
        }

        public async Task BulkInsertAsync(
            IEnumerable<CargaFallo> fallos,
            CancellationToken cancellationToken = default)
        {
            var fallosList = fallos.ToList();

            if (!fallosList.Any())
                return;

            await _context.CargasFallos.AddRangeAsync(fallosList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<CargaFallo>> GetByCargaIdAsync(
            int idCarga,
            CancellationToken cancellationToken = default)
        {
            return await _context.CargasFallos
                .Where(f => f.IdCarga == idCarga)
                .OrderBy(f => f.RowNumber)
                .ToListAsync(cancellationToken);
        }
    }
}
