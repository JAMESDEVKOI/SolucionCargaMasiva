using BulkProcessor.Domain.Entities;
using BulkProcessor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BulkProcessor.Infrastructure.Persistence.Repositories
{
    public class DataProcesadaRepository : IDataProcesadaRepository
    {
        private readonly BulkProcessorDbContext _context;

        public DataProcesadaRepository(BulkProcessorDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByCodigoProductoAsync(
            string codigoProducto,
            CancellationToken cancellationToken = default)
        {
            return await _context.DataProcesada
                .AnyAsync(d => d.CodigoProducto == codigoProducto, cancellationToken);
        }

        public async Task<int> BulkInsertAsync(
            IEnumerable<DataProcesada> data,
            CancellationToken cancellationToken = default)
        {
            var dataList = data.ToList();

            if (!dataList.Any())
                return 0;

            await _context.DataProcesada.AddRangeAsync(dataList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return dataList.Count;
        }
    }
}
