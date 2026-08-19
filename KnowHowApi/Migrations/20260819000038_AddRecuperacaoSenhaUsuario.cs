using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnowHowApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRecuperacaoSenhaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RecuperacaoSenhaCodigoExpiraEm",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecuperacaoSenhaCodigoHash",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecuperacaoSenhaTentativas",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecuperacaoSenhaTokenExpiraEm",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecuperacaoSenhaTokenHash",
                table: "Usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecuperacaoSenhaCodigoExpiraEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RecuperacaoSenhaCodigoHash",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RecuperacaoSenhaTentativas",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RecuperacaoSenhaTokenExpiraEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RecuperacaoSenhaTokenHash",
                table: "Usuarios");
        }
    }
}
