using FileControl.Domain.Entities;
using FileControl.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileControl.Infrastructure.Persistence
{
    public class FileControlDbContext : DbContext, IUnitOfWork
    {
        public FileControlDbContext(DbContextOptions<FileControlDbContext> options)
            : base(options)
        {
        }

        public DbSet<CargaArchivo> CargasArchivos => Set<CargaArchivo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CargaArchivo>(entity =>
            {
                entity.ToTable("cargas_archivos");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.NombreArchivo)
                    .HasColumnName("nombre_archivo")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.Usuario)
                    .HasColumnName("usuario")
                    .HasMaxLength(400)
                    .IsRequired();

                entity.Property(e => e.Periodo)
                    .HasColumnName("periodo")
                    .HasMaxLength(7)
                    .IsRequired();

                entity.Property(e => e.FileId)
                    .HasColumnName("file_id")
                    .HasMaxLength(500);

                entity.Property(e => e.Estado)
                    .HasColumnName("estado")
                    .IsRequired();

                entity.Property(e => e.FechaRegistro)
                    .HasColumnName("fecha_registro")
                    .IsRequired();

                entity.Property(e => e.FechaInicioProceso)
                    .HasColumnName("fecha_inicio_proceso");

                entity.Property(e => e.FechaFinProceso)
                    .HasColumnName("fecha_fin_proceso");

                entity.Property(e => e.MensajeError)
                    .HasColumnName("mensaje_error")
                    .HasMaxLength(2000);

                entity.Property(e => e.TotalRegistros)
                    .HasColumnName("total_registros")
                    .IsRequired();

                entity.Property(e => e.RegistrosProcesados)
                    .HasColumnName("registros_procesados")
                    .IsRequired();

                entity.Property(e => e.RegistrosExitosos)
                    .HasColumnName("registros_exitosos")
                    .IsRequired();

                entity.Property(e => e.RegistrosFallidos)
                    .HasColumnName("registros_fallidos")
                    .IsRequired();

                entity.HasIndex(e => e.Usuario)
                    .HasDatabaseName("ix_cargas_archivos_usuario");

                entity.HasIndex(e => e.Periodo)
                    .HasDatabaseName("ix_cargas_archivos_periodo");

                entity.HasIndex(e => e.Estado)
                    .HasDatabaseName("ix_cargas_archivos_estado");
            });
        }
    }
}
