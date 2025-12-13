using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FileControl.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cargas_archivos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_archivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    usuario = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    periodo = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    file_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_inicio_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_fin_proceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mensaje_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    total_registros = table.Column<int>(type: "integer", nullable: false),
                    registros_procesados = table.Column<int>(type: "integer", nullable: false),
                    registros_exitosos = table.Column<int>(type: "integer", nullable: false),
                    registros_fallidos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargas_archivos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cargas_archivos_estado",
                table: "cargas_archivos",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_cargas_archivos_periodo",
                table: "cargas_archivos",
                column: "periodo");

            migrationBuilder.CreateIndex(
                name: "ix_cargas_archivos_usuario",
                table: "cargas_archivos",
                column: "usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cargas_archivos");
        }
    }
}
