using BulkProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BulkProcessor.Infrastructure.Persistence.Configurations
{
    public class CargaFalloConfiguration : IEntityTypeConfiguration<CargaFallo>
    {
        public void Configure(EntityTypeBuilder<CargaFallo> builder)
        {
            builder.ToTable("cargas_fallos");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                .HasColumnName("id");

            builder.Property(f => f.IdCarga)
                .IsRequired()
                .HasColumnName("id_carga");

            builder.Property(f => f.RowNumber)
                .IsRequired()
                .HasColumnName("row_number");

            builder.Property(f => f.CodigoProducto)
                .HasMaxLength(50)
                .HasColumnName("codigo_producto");

            builder.Property(f => f.Motivo)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("motivo");

            builder.Property(f => f.RawData)
                .HasColumnType("text")
                .HasColumnName("raw_data");

            builder.Property(f => f.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.HasIndex(f => f.IdCarga)
                .HasDatabaseName("ix_cargas_fallos_id_carga");
            
            builder.HasIndex(f => new { f.IdCarga, f.RowNumber })
                .HasDatabaseName("ix_cargas_fallos_id_carga_row");
        }
    }
}
