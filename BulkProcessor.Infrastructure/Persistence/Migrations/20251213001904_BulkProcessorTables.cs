using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BulkProcessor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BulkProcessorTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SKIP: No crear cargas_archivos - ya existe (creada por FileControl)
            // migrationBuilder.CreateTable(
            //     name: "cargas_archivos",
            //     columns: table => new
            //     {
            //         id = table.Column<int>(type: "integer", nullable: false)
            //             .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //         nombre_archivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
            //         usuario = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
            //         periodo = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
            //         file_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
            //         estado = table.Column<int>(type: "integer", nullable: false),
            //         fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         fecha_inicio_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         fecha_fin_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         mensaje_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
            //         total_registros = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
            //         registros_procesados = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
            //         registros_exitosos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
            //         registros_fallidos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_cargas_archivos", x => x.id);
            //     });

            migrationBuilder.CreateTable(
                name: "data_procesada",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    periodo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    codigo_producto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre_producto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    precio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    stock = table.Column<int>(type: "integer", nullable: true),
                    proveedor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_procesada", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cargas_fallos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_carga = table.Column<int>(type: "integer", nullable: false),
                    row_number = table.Column<int>(type: "integer", nullable: false),
                    codigo_producto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    raw_data = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CargaArchivoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargas_fallos", x => x.id);
                    table.ForeignKey(
                        name: "FK_cargas_fallos_cargas_archivos_CargaArchivoId",
                        column: x => x.CargaArchivoId,
                        principalTable: "cargas_archivos",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_cargas_fallos_CargaArchivoId",
                table: "cargas_fallos",
                column: "CargaArchivoId");

            migrationBuilder.CreateIndex(
                name: "ix_cargas_fallos_id_carga",
                table: "cargas_fallos",
                column: "id_carga");

            migrationBuilder.CreateIndex(
                name: "ix_cargas_fallos_id_carga_row",
                table: "cargas_fallos",
                columns: new[] { "id_carga", "row_number" });

            migrationBuilder.CreateIndex(
                name: "ix_data_procesada_codigo_producto_unique",
                table: "data_procesada",
                column: "codigo_producto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_procesada_periodo",
                table: "data_procesada",
                column: "periodo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cargas_fallos");

            migrationBuilder.DropTable(
                name: "data_procesada");

            migrationBuilder.DropTable(
                name: "cargas_archivos");
        }
    }
}
