using BulkProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BulkProcessor.Infrastructure.Persistence.Configurations
{
    public class DataProcesadaConfiguration : IEntityTypeConfiguration<DataProcesada>
    {
        public void Configure(EntityTypeBuilder<DataProcesada> builder)
        {
            builder.ToTable("data_procesada");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .HasColumnName("id");

            builder.Property(d => d.Periodo)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("periodo");

            builder.Property(d => d.CodigoProducto)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("codigo_producto");

            builder.Property(d => d.NombreProducto)
                .HasMaxLength(200)
                .HasColumnName("nombre_producto");

            builder.Property(d => d.Categoria)
                .HasMaxLength(100)
                .HasColumnName("categoria");

            builder.Property(d => d.Precio)
                .HasPrecision(18, 2)
                .HasColumnName("precio");

            builder.Property(d => d.Stock)
                .HasColumnName("stock");

            builder.Property(d => d.Proveedor)
                .HasMaxLength(200)
                .HasColumnName("proveedor");

            builder.Property(d => d.Descripcion)
                .HasMaxLength(1000)
                .HasColumnName("descripcion");

            builder.Property(d => d.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.HasIndex(d => d.CodigoProducto)
                .IsUnique()
                .HasDatabaseName("ix_data_procesada_codigo_producto_unique");

            builder.HasIndex(d => d.Periodo)
                .HasDatabaseName("ix_data_procesada_periodo");
        }
    }
}
