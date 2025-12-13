using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations
{
    public class CargaArchivoConfiguration : IEntityTypeConfiguration<CargaArchivo>
    {
        public void Configure(EntityTypeBuilder<CargaArchivo> builder)
        {
            // Usar la MISMA tabla que FileControl y BulkProcessor
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
                .HasColumnName("total_registros");

            builder.Property(c => c.RegistrosProcesados)
                .HasColumnName("registros_procesados");

            builder.Property(c => c.RegistrosExitosos)
                .HasColumnName("registros_exitosos");

            builder.Property(c => c.RegistrosFallidos)
                .HasColumnName("registros_fallidos");

            // Campos adicionales para notificaciones
            builder.Property(c => c.NotificadoAt)
                .HasColumnName("notificado_at");

            builder.Property(c => c.EmailStatus)
                .HasMaxLength(50)
                .HasColumnName("email_status");

            builder.Property(c => c.EmailError)
                .HasMaxLength(1000)
                .HasColumnName("email_error");
        }
    }
}
