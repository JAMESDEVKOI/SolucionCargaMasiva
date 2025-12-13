using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SKIP: No crear tabla cargas_archivos - ya existe (creada por FileControl)
            // Solo agregar las columnas nuevas para notificaciones

            migrationBuilder.AddColumn<DateTime>(
                name: "notificado_at",
                table: "cargas_archivos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_status",
                table: "cargas_archivos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_error",
                table: "cargas_archivos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "notificado_at",
                table: "cargas_archivos");

            migrationBuilder.DropColumn(
                name: "email_status",
                table: "cargas_archivos");

            migrationBuilder.DropColumn(
                name: "email_error",
                table: "cargas_archivos");
        }
    }
}
