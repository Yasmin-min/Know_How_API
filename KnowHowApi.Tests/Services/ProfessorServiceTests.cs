using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class ProfessorServiceTests
    {
        private readonly Mock<IProfessorRepository> _professorRepositoryMock = new();
        private readonly ProfessorService _service;

        public ProfessorServiceTests()
        {
            _service = new ProfessorService(_professorRepositoryMock.Object);
        }

        private static PerfilProfessor NovoPerfil(int id = 1, int usuarioId = 10) => new()
        {
            Id = id,
            UsuarioId = usuarioId,
            Materia = "Desenvolvimento Web",
            Avaliacao = 4.9m,
            TotalAvaliacoes = 3,
            Descricao = "Descrição",
            AvatarVariante = "padrao"
        };

        [Fact]
        public async Task ObterDashboard_ProfessorComPerfil_RetornaEspecialidadeEAvaliacaoReaisComIndicadoresZerados()
        {
            var perfil = NovoPerfil();
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(10)).ReturnsAsync(perfil);

            var dashboard = await _service.ObterDashboard(10);

            Assert.NotNull(dashboard);
            Assert.Equal("Desenvolvimento Web", dashboard!.Especialidade);
            Assert.Equal(4.9m, dashboard.Indicadores.AvaliacaoMedia);
            Assert.Equal(0, dashboard.Indicadores.AulasAtivas);
            Assert.Equal(0, dashboard.Indicadores.Alunos);
            Assert.Equal(0, dashboard.Indicadores.AulasSemana);
            Assert.Empty(dashboard.ProximasAulas);
            Assert.Empty(dashboard.AtividadesRecentes);
            Assert.Empty(dashboard.MinhasAulas);
        }

        [Fact]
        public async Task ObterDashboard_ProfessorSemPerfil_RetornaNull()
        {
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(99)).ReturnsAsync((PerfilProfessor?)null);

            var dashboard = await _service.ObterDashboard(99);

            Assert.Null(dashboard);
        }
    }
}
