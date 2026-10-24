using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KnowHowApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanosPrecosContratacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesPrecos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfessorId = table.Column<int>(type: "integer", nullable: false),
                    AulaExperimentalTipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AulaExperimentalPercentualDesconto = table.Column<int>(type: "integer", nullable: true),
                    AulaExperimentalDuracaoMinutos = table.Column<int>(type: "integer", nullable: false),
                    AtualizadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PrecosPorDuracao = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesPrecos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracoesPrecos_Usuarios_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contratacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessorId = table.Column<int>(type: "integer", nullable: false),
                    AlunoId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OfertaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAulas = table.Column<int>(type: "integer", nullable: false),
                    AulasUsadas = table.Column<int>(type: "integer", nullable: false),
                    Inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    Fim = table.Column<DateOnly>(type: "date", nullable: true),
                    StatusPagamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PagoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contratacoes_Usuarios_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contratacoes_Usuarios_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pacotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessorId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    QuantidadeAulas = table.Column<int>(type: "integer", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "integer", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    ValidadeDias = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pacotes_Usuarios_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanosMensais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessorId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    AulasPorSemana = table.Column<int>(type: "integer", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "integer", nullable: false),
                    ValorMensal = table.Column<decimal>(type: "numeric", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosMensais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanosMensais_Usuarios_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesPrecos_ProfessorId",
                table: "ConfiguracoesPrecos",
                column: "ProfessorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contratacoes_AlunoId",
                table: "Contratacoes",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratacoes_ProfessorId_CriadaEm",
                table: "Contratacoes",
                columns: new[] { "ProfessorId", "CriadaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Pacotes_ProfessorId",
                table: "Pacotes",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosMensais_ProfessorId",
                table: "PlanosMensais",
                column: "ProfessorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesPrecos");

            migrationBuilder.DropTable(
                name: "Contratacoes");

            migrationBuilder.DropTable(
                name: "Pacotes");

            migrationBuilder.DropTable(
                name: "PlanosMensais");
        }
    }
}
