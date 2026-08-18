using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KnowHowApi.Migrations
{
    /// <inheritdoc />
    public partial class InicialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AreasInteresse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreasInteresse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    SenhaHash = table.Column<string>(type: "text", nullable: false),
                    TipoUsuario = table.Column<int>(type: "integer", nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "date", nullable: false),
                    Cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    AreaInteresseId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_AreasInteresse_AreaInteresseId",
                        column: x => x.AreaInteresseId,
                        principalTable: "AreasInteresse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilProfessores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Materia = table.Column<string>(type: "text", nullable: false),
                    ValorHora = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Avaliacao = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    TotalAvaliacoes = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Disponivel = table.Column<bool>(type: "boolean", nullable: false),
                    AvatarVariante = table.Column<string>(type: "text", nullable: false),
                    AreaEspecialidadeId = table.Column<int>(type: "integer", nullable: true),
                    AnosExperiencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FotoPerfil = table.Column<byte[]>(type: "bytea", nullable: true),
                    FotoPerfilContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilProfessores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilProfessores_AreasInteresse_AreaEspecialidadeId",
                        column: x => x.AreaEspecialidadeId,
                        principalTable: "AreasInteresse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerfilProfessores_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_AreasInteresse_Nome",
                table: "AreasInteresse",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilProfessores_AreaEspecialidadeId",
                table: "PerfilProfessores",
                column: "AreaEspecialidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilProfessores_UsuarioId",
                table: "PerfilProfessores",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AreaInteresseId",
                table: "Usuarios",
                column: "AreaInteresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Cpf",
                table: "Usuarios",
                column: "Cpf",
                unique: true,
                filter: "\"Cpf\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfilProfessores");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "AreasInteresse");
        }
    }
}
