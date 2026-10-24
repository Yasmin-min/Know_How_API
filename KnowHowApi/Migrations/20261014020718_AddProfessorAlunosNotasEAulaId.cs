using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KnowHowApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessorAlunosNotasEAulaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AulaId",
                table: "AulasAgendadas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotasAluno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfessorId = table.Column<int>(type: "integer", nullable: false),
                    AlunoId = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AtualizadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasAluno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasAluno_Usuarios_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasAluno_Usuarios_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProfessorAlunos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfessorId = table.Column<int>(type: "integer", nullable: false),
                    AlunoId = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessorAlunos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfessorAlunos_Usuarios_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfessorAlunos_Usuarios_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AulasAgendadas_AulaId",
                table: "AulasAgendadas",
                column: "AulaId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasAluno_AlunoId",
                table: "NotasAluno",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasAluno_ProfessorId_AlunoId",
                table: "NotasAluno",
                columns: new[] { "ProfessorId", "AlunoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfessorAlunos_AlunoId",
                table: "ProfessorAlunos",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessorAlunos_ProfessorId_AlunoId",
                table: "ProfessorAlunos",
                columns: new[] { "ProfessorId", "AlunoId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AulasAgendadas_Aulas_AulaId",
                table: "AulasAgendadas",
                column: "AulaId",
                principalTable: "Aulas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AulasAgendadas_Aulas_AulaId",
                table: "AulasAgendadas");

            migrationBuilder.DropTable(
                name: "NotasAluno");

            migrationBuilder.DropTable(
                name: "ProfessorAlunos");

            migrationBuilder.DropIndex(
                name: "IX_AulasAgendadas_AulaId",
                table: "AulasAgendadas");

            migrationBuilder.DropColumn(
                name: "AulaId",
                table: "AulasAgendadas");
        }
    }
}
