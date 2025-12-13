using FileControl.Domain.Entities;
using FileControl.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileControl.Infrastructure.Persistence.Repositories
{
    public class CargaArchivoRepository : ICargaArchivoRepository
    {
        private readonly FileControlDbContext _context;

        public CargaArchivoRepository(FileControlDbContext context)
        {
            _context = context;
        }

        public async Task<CargaArchivo> CreateAsync(
            CargaArchivo cargaArchivo,
            CancellationToken cancellationToken = default)
        {
            await _context.CargasArchivos.AddAsync(cargaArchivo, cancellationToken);
            return cargaArchivo;
        }

        public async Task<CargaArchivo?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.CargasArchivos
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<bool> UpdateAsync(
            CargaArchivo cargaArchivo,
            CancellationToken cancellationToken = default)
        {
            _context.CargasArchivos.Update(cargaArchivo);
            return await Task.FromResult(true);
        }

        public async Task<IEnumerable<CargaArchivo>> GetByUsuarioAsync(
            string usuario,
            CancellationToken cancellationToken = default)
        {
            return await _context.CargasArchivos
                .Where(c => c.Usuario == usuario)
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CargaArchivo>> GetByPeriodoAsync(
            string periodo,
            CancellationToken cancellationToken = default)
        {
            return await _context.CargasArchivos
                .Where(c => c.Periodo == periodo)
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync(cancellationToken);
        }
    }
}
