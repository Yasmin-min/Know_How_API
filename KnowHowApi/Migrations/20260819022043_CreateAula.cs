using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KnowHowApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateAula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aulas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfessorId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AreaInteresseId = table.Column<int>(type: "integer", nullable: true),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DuracaoMinutos = table.Column<int>(type: "integer", nullable: true),
                    Nivel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aulas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aulas_AreasInteresse_AreaInteresseId",
                        column: x => x.AreaInteresseId,
                        principalTable: "AreasInteresse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aulas_Usuarios_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_AreaInteresseId",
                table: "Aulas",
                column: "AreaInteresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_ProfessorId_Status",
                table: "Aulas",
                columns: new[] { "ProfessorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_ProfessorId_Titulo",
                table: "Aulas",
                columns: new[] { "ProfessorId", "Titulo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aulas");
        }
    }
}
