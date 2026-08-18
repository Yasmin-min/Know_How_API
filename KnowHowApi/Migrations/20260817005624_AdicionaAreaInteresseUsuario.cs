using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KnowHowApi.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAreaInteresseUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaInteresseId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AreasInteresse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreasInteresse", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AreasInteresse",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { 1, "Tecnologia e Programação" },
                    { 2, "Idiomas" },
                    { 3, "Música" },
                    { 4, "Design e Artes" },
                    { 5, "Negócios e Empreendedorismo" },
                    { 6, "Saúde e Bem-estar" },
                    { 7, "Reforço Escolar" },
                    { 8, "Outros" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AreaInteresseId",
                table: "Usuarios",
                column: "AreaInteresseId");

            migrationBuilder.CreateIndex(
                name: "IX_AreasInteresse_Nome",
                table: "AreasInteresse",
                column: "Nome",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_AreasInteresse_AreaInteresseId",
                table: "Usuarios",
                column: "AreaInteresseId",
                principalTable: "AreasInteresse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_AreasInteresse_AreaInteresseId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "AreasInteresse");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_AreaInteresseId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "AreaInteresseId",
                table: "Usuarios");
        }
    }
}
