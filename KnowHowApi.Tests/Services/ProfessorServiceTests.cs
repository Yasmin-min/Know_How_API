using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class ProfessorServiceTests
    {
        private readonly Mock<IProfessorRepository> _professorRepositoryMock = new();
        private readonly Mock<IAreaInteresseRepository> _areaInteresseRepositoryMock = new();
        private readonly Mock<IAulaRepository> _aulaRepositoryMock = new();
        private readonly ProfessorService _service;

        public ProfessorServiceTests()
        {
            _service = new ProfessorService(_professorRepositoryMock.Object, _areaInteresseRepositoryMock.Object, _aulaRepositoryMock.Object);
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

        private static CriarAulaRequestDTO AulaAtivaValida() => new()
        {
            Titulo = "Introdução ao React",
            Descricao = "Aprenda React.",
            Materia = "Tecnologia e Programação",
            Valor = 150m,
            DuracaoMinutos = 90,
            Nivel = "Iniciante",
            Status = "ativa"
        };

        private static CriarAulaRequestDTO RascunhoIncompleto() => new()
        {
            Titulo = "",
            Descricao = "",
            Materia = null,
            Valor = null,
            DuracaoMinutos = null,
            Nivel = null,
            Status = "rascunho"
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

        [Fact]
        public async Task CriarAula_StatusAtivaComDadosValidos_PersisteEVinculaAoProfessorAutenticado()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Tecnologia e Programação"))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _aulaRepositoryMock.Setup(r => r.CriarAula(It.IsAny<Aula>()))
                .ReturnsAsync((Aula a) => { a.Id = 7; return a; });

            var response = await _service.CriarAula(10, AulaAtivaValida());

            Assert.Equal(7, response.Id);
            Assert.Equal("Introdução ao React", response.Titulo);
            Assert.Equal("Tecnologia e Programação", response.Materia);
            Assert.Equal(150m, response.Valor);
            Assert.Equal(90, response.DuracaoMinutos);
            Assert.Equal("Iniciante", response.Nivel);
            Assert.Equal("ativa", response.Status);
            Assert.Equal(0, response.TotalAlunos);

            _aulaRepositoryMock.Verify(r => r.CriarAula(It.Is<Aula>(a =>
                a.ProfessorId == 10 &&
                a.AreaInteresseId == 1 &&
                a.Status == "ativa")), Times.Once);
        }

        [Fact]
        public async Task CriarAula_RascunhoIncompleto_NaoAplicaValidacoesObrigatorias()
        {
            _aulaRepositoryMock.Setup(r => r.CriarAula(It.IsAny<Aula>()))
                .ReturnsAsync((Aula a) => { a.Id = 8; return a; });

            var response = await _service.CriarAula(10, RascunhoIncompleto());

            Assert.Equal("rascunho", response.Status);
            Assert.Equal("", response.Titulo);
            Assert.Equal("", response.Descricao);
            Assert.Null(response.Materia);
            Assert.Null(response.Valor);
            Assert.Null(response.DuracaoMinutos);
            Assert.Null(response.Nivel);
            _areaInteresseRepositoryMock.Verify(r => r.GetAreaInteresseByNome(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_AtivaSemTitulo_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Titulo = "   ";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_AtivaSemDescricao_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Descricao = "";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_AtivaSemMateria_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Materia = null;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_AtivaSemValor_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Valor = null;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_AtivaComValorZero_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Valor = 0;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_ValorNegativoNoRascunho_LancaDomainValidationException()
        {
            var request = RascunhoIncompleto();
            request.Valor = -1;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
        }

        [Fact]
        public async Task CriarAula_TituloComMaisDeCemCaracteres_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Titulo = new string('a', 101);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
        }

        [Fact]
        public async Task CriarAula_DescricaoComMaisDeQuinhentosCaracteres_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Descricao = new string('a', 501);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
        }

        [Fact]
        public async Task CriarAula_MateriaInexistente_LancaDomainValidationException()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Área que não existe"))
                .ReturnsAsync((AreaInteresse?)null);
            var request = AulaAtivaValida();
            request.Materia = "Área que não existe";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_NivelInvalido_LancaDomainValidationException()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome(It.IsAny<string>()))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            var request = AulaAtivaValida();
            request.Nivel = "Expert";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_DuracaoForaDosValoresPermitidos_LancaDomainValidationException()
        {
            var request = RascunhoIncompleto();
            request.DuracaoMinutos = 75;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_StatusInvalido_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Status = "publicada";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_StatusPausada_LancaDomainValidationException()
        {
            var request = AulaAtivaValida();
            request.Status = "pausada";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, request));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_TituloDuplicadoParaOMesmoProfessor_LancaDomainValidationException()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome(It.IsAny<string>()))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _aulaRepositoryMock.Setup(r => r.ExisteAulaComTitulo(10, "Introdução ao React")).ReturnsAsync(true);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(10, AulaAtivaValida()));
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_MesmoTituloParaProfessoresDiferentes_NaoBloqueia()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome(It.IsAny<string>()))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _aulaRepositoryMock.Setup(r => r.ExisteAulaComTitulo(10, "Introdução ao React")).ReturnsAsync(false);
            _aulaRepositoryMock.Setup(r => r.CriarAula(It.IsAny<Aula>())).ReturnsAsync((Aula a) => a);

            await _service.CriarAula(10, AulaAtivaValida());

            _aulaRepositoryMock.Verify(r => r.CriarAula(It.IsAny<Aula>()), Times.Once);
        }

        [Fact]
        public async Task CriarAula_DoisProfessores_AulaFicaVinculadaSomenteAoProfessorAutenticado()
        {
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Tecnologia e Programação"))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _aulaRepositoryMock.Setup(r => r.CriarAula(It.IsAny<Aula>())).ReturnsAsync((Aula a) => a);

            await _service.CriarAula(10, AulaAtivaValida());

            _aulaRepositoryMock.Verify(r => r.CriarAula(It.Is<Aula>(a => a.ProfessorId == 10)), Times.Once);
            _aulaRepositoryMock.Verify(r => r.CriarAula(It.Is<Aula>(a => a.ProfessorId == 20)), Times.Never);
        }
    }
}
