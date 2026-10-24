using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnowHowApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFotoPerfilUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FotoPerfil",
                table: "Usuarios",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoPerfilContentType",
                table: "Usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoPerfil",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FotoPerfilContentType",
                table: "Usuarios");
        }
    }
}
