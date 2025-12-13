using BulkProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BulkProcessor.Infrastructure.Persistence
{
    public class BulkProcessorDbContext : DbContext
    {
        public BulkProcessorDbContext(DbContextOptions<BulkProcessorDbContext> options)
            : base(options)
        {
        }

        public DbSet<CargaArchivo> CargasArchivo { get; set; } = null!;
        public DbSet<DataProcesada> DataProcesada { get; set; } = null!;
        public DbSet<CargaFallo> CargasFallos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BulkProcessorDbContext).Assembly);
        }
    }
}
