using BulkProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BulkProcessor.Infrastructure.Persistence.Configurations
{
    public class CargaArchivoConfiguration : IEntityTypeConfiguration<CargaArchivo>
    {
        public void Configure(EntityTypeBuilder<CargaArchivo> builder)
        {
            // IMPORTANTE: Usar la MISMA tabla que FileControl
            builder.ToTable("cargas_archivos");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .HasColumnName("id");

            builder.Property(c => c.NombreArchivo)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("nombre_archivo");

            builder.Property(c => c.Usuario)
                .IsRequired()
                .HasMaxLength(400)
                .HasColumnName("usuario");

            builder.Property(c => c.Periodo)
                .IsRequired()
                .HasMaxLength(7)
                .HasColumnName("periodo");

            builder.Property(c => c.FileId)
                .HasMaxLength(500)
                .HasColumnName("file_id");

            builder.Property(c => c.Estado)
                .IsRequired()
                // NO usar conversión a string, FileControl usa integer
                .HasColumnName("estado");

            builder.Property(c => c.FechaRegistro)
                .IsRequired()
                .HasColumnName("fecha_registro");

            builder.Property(c => c.FechaInicioProceso)
                .HasColumnName("fecha_inicio_proceso");

            builder.Property(c => c.FechaFinProceso)
                .HasColumnName("fecha_fin_proceso");

            builder.Property(c => c.MensajeError)
                .HasMaxLength(2000)
                .HasColumnName("mensaje_error");

            builder.Property(c => c.TotalRegistros)
                .HasColumnName("total_registros")
                .HasDefaultValue(0);

            builder.Property(c => c.RegistrosProcesados)
                .HasColumnName("registros_procesados")
                .HasDefaultValue(0);

            builder.Property(c => c.RegistrosExitosos)
                .HasColumnName("registros_exitosos")
                .HasDefaultValue(0);

            builder.Property(c => c.RegistrosFallidos)
                .HasColumnName("registros_fallidos")
                .HasDefaultValue(0);

            // Índices ya creados por FileControl, no duplicar
            // builder.HasIndex(c => new { c.Periodo, c.Estado })
            //     .HasDatabaseName("ix_cargas_archivos_periodo_estado");

            // builder.HasIndex(c => c.Estado)
            //     .HasDatabaseName("ix_cargas_archivos_estado");
        }
    }
}
