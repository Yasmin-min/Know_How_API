using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnowHowApi.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCadastroCompletoProfessor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnosExperiencia",
                table: "PerfilProfessores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AreaEspecialidadeId",
                table: "PerfilProfessores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FotoPerfil",
                table: "PerfilProfessores",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoPerfilContentType",
                table: "PerfilProfessores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilProfessores_AreaEspecialidadeId",
                table: "PerfilProfessores",
                column: "AreaEspecialidadeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PerfilProfessores_AreasInteresse_AreaEspecialidadeId",
                table: "PerfilProfessores",
                column: "AreaEspecialidadeId",
                principalTable: "AreasInteresse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PerfilProfessores_AreasInteresse_AreaEspecialidadeId",
                table: "PerfilProfessores");

            migrationBuilder.DropIndex(
                name: "IX_PerfilProfessores_AreaEspecialidadeId",
                table: "PerfilProfessores");

            migrationBuilder.DropColumn(
                name: "AnosExperiencia",
                table: "PerfilProfessores");

            migrationBuilder.DropColumn(
                name: "AreaEspecialidadeId",
                table: "PerfilProfessores");

            migrationBuilder.DropColumn(
                name: "FotoPerfil",
                table: "PerfilProfessores");

            migrationBuilder.DropColumn(
                name: "FotoPerfilContentType",
                table: "PerfilProfessores");
        }
    }
}
