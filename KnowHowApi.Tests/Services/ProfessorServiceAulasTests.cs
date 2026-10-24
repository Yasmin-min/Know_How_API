using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services;
using Moq;

namespace KnowHowApi.Tests.Services
{
    // GET /Professor/aulas, GET e PUT /Professor/aulas/{id} e PUT /Professor/aulas/{id}/status.
    public class ProfessorServiceAulasTests
    {
        private const int ProfessorId = 10;
        private const int AulaId = 31;

        private readonly Mock<IProfessorRepository> _professorRepositoryMock = new();
        private readonly Mock<IAreaInteresseRepository> _areaInteresseRepositoryMock = new();
        private readonly Mock<IAulaRepository> _aulaRepositoryMock = new();
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
        private readonly Mock<IAulaAgendadaRepository> _aulaAgendadaRepositoryMock = new();
        private readonly Mock<IAvaliacaoRepository> _avaliacaoRepositoryMock = new();
        private readonly ProfessorService _service;

        private static readonly AreaInteresse Programacao = new() { Id = 3, Nome = "Programação" };

        public ProfessorServiceAulasTests()
        {
            _service = new ProfessorService(_professorRepositoryMock.Object, _areaInteresseRepositoryMock.Object, _aulaRepositoryMock.Object, _usuarioRepositoryMock.Object, _aulaAgendadaRepositoryMock.Object, _avaliacaoRepositoryMock.Object);

            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Programação")).ReturnsAsync(Programacao);
            _aulaRepositoryMock.Setup(r => r.AtualizarAula(It.IsAny<Aula>())).ReturnsAsync((Aula a) => a);
            _aulaAgendadaRepositoryMock.Setup(r => r.ContarAlunosAtendidos(ProfessorId, It.IsAny<DateTime>())).ReturnsAsync(4);
        }

        private static Aula AulaCompleta(string status) => new()
        {
            Id = AulaId,
            ProfessorId = ProfessorId,
            Titulo = "Introdução ao React",
            Descricao = "Componentes, props e estado.",
            AreaInteresseId = Programacao.Id,
            AreaInteresse = Programacao,
            Valor = 150m,
            DuracaoMinutos = 60,
            Nivel = "Iniciante",
            Status = status,
            CriadaEm = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        private static Aula RascunhoIncompleto() => new()
        {
            Id = AulaId,
            ProfessorId = ProfessorId,
            Titulo = "Rascunho",
            Descricao = "",
            Status = "rascunho",
            CriadaEm = DateTime.UtcNow
        };

        private void AulaExiste(Aula aula) =>
            _aulaRepositoryMock.Setup(r => r.GetAulaDoProfessor(aula.Id, ProfessorId)).ReturnsAsync(aula);

        private static CriarAulaRequestDTO EdicaoValida(string status = "ativa") => new()
        {
            Titulo = "Introdução ao React",
            Descricao = "Componentes, props e estado.",
            Materia = "Programação",
            Valor = 150m,
            DuracaoMinutos = 60,
            Nivel = "Iniciante",
            Status = status
        };

        [Fact]
        public async Task ListarAulas_RetornaTodosOsStatusComTotalAlunosEMateria()
        {
            _aulaRepositoryMock.Setup(r => r.ListarDoProfessor(ProfessorId)).ReturnsAsync(new List<Aula>
            {
                AulaCompleta("pausada"),
                RascunhoIncompleto()
            });

            var aulas = await _service.ListarAulas(ProfessorId);

            Assert.Equal(new[] { "pausada", "rascunho" }, aulas.Select(a => a.Status));
            Assert.All(aulas, a => Assert.Equal(4, a.TotalAlunos));
            Assert.Equal("Programação", aulas[0].Materia);
            Assert.Null(aulas[1].Materia);
        }

        [Fact]
        public async Task ObterAula_DeOutroProfessorOuInexistente_LancaNotFound()
        {
            _aulaRepositoryMock.Setup(r => r.GetAulaDoProfessor(AulaId, ProfessorId)).ReturnsAsync((Aula?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterAula(ProfessorId, AulaId));
            Assert.Equal("Aula não encontrada.", ex.Message);
        }

        [Fact]
        public async Task AtualizarAula_MesmoTitulo_IgnoraAPropriaAulaNaChecagemDeDuplicidade()
        {
            AulaExiste(AulaCompleta("ativa"));
            _aulaRepositoryMock.Setup(r => r.ExisteAulaComTitulo(ProfessorId, "Introdução ao React", AulaId)).ReturnsAsync(false);

            var response = await _service.AtualizarAula(ProfessorId, AulaId, EdicaoValida());

            Assert.Equal("Introdução ao React", response.Titulo);
            _aulaRepositoryMock.Verify(r => r.ExisteAulaComTitulo(ProfessorId, "Introdução ao React", AulaId), Times.Once);
            _aulaRepositoryMock.Verify(r => r.AtualizarAula(It.IsAny<Aula>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarAula_TituloDeOutraAula_LancaValidacao()
        {
            AulaExiste(AulaCompleta("ativa"));
            _aulaRepositoryMock.Setup(r => r.ExisteAulaComTitulo(ProfessorId, "Introdução ao React", AulaId)).ReturnsAsync(true);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarAula(ProfessorId, AulaId, EdicaoValida()));
            _aulaRepositoryMock.Verify(r => r.AtualizarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarAula_PausadaSemValor_LancaValidacao()
        {
            AulaExiste(AulaCompleta("pausada"));
            var request = EdicaoValida("pausada");
            request.Valor = null;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarAula(ProfessorId, AulaId, request));
            _aulaRepositoryMock.Verify(r => r.AtualizarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarAula_RascunhoIncompleto_Salva()
        {
            AulaExiste(RascunhoIncompleto());
            var request = new CriarAulaRequestDTO { Titulo = "", Descricao = "", Status = "rascunho" };

            var response = await _service.AtualizarAula(ProfessorId, AulaId, request);

            Assert.Equal("rascunho", response.Status);
            _aulaRepositoryMock.Verify(r => r.AtualizarAula(It.IsAny<Aula>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarAula_AplicaCamposEStatus()
        {
            AulaExiste(AulaCompleta("ativa"));
            var request = EdicaoValida("pausada");
            request.Valor = 99m;

            var response = await _service.AtualizarAula(ProfessorId, AulaId, request);

            Assert.Equal("pausada", response.Status);
            Assert.Equal(99m, response.Valor);
            Assert.Equal("Programação", response.Materia);
            _aulaRepositoryMock.Verify(r => r.AtualizarAula(It.Is<Aula>(a => a.Valor == 99m && a.AreaInteresseId == Programacao.Id)), Times.Once);
        }

        [Fact]
        public async Task AtualizarAula_Inexistente_LancaNotFound()
        {
            _aulaRepositoryMock.Setup(r => r.GetAulaDoProfessor(AulaId, ProfessorId)).ReturnsAsync((Aula?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.AtualizarAula(ProfessorId, AulaId, EdicaoValida()));
        }

        [Theory]
        [InlineData("ativa", "pausada")]
        [InlineData("pausada", "ativa")]
        [InlineData("rascunho", "ativa")]
        public async Task AlterarStatus_TransicaoPermitida_Atualiza(string de, string para)
        {
            AulaExiste(AulaCompleta(de));

            var response = await _service.AlterarStatusAula(ProfessorId, AulaId, new AlterarStatusAulaRequestDTO { Status = para });

            Assert.Equal(para, response.Status);
            _aulaRepositoryMock.Verify(r => r.AtualizarAula(It.Is<Aula>(a => a.Status == para)), Times.Once);
        }

        [Theory]
        [InlineData("ativa", "rascunho")]
        [InlineData("pausada", "rascunho")]
        [InlineData("rascunho", "pausada")]
        public async Task AlterarStatus_TransicaoNaoPermitida_LancaValidacao(string de, string para)
        {
            AulaExiste(AulaCompleta(de));

            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.AlterarStatusAula(ProfessorId, AulaId, new AlterarStatusAulaRequestDTO { Status = para }));
            _aulaRepositoryMock.Verify(r => r.AtualizarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task AlterarStatus_PublicarRascunhoIncompleto_LancaMensagemClara()
        {
            AulaExiste(RascunhoIncompleto());

            var ex = await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.AlterarStatusAula(ProfessorId, AulaId, new AlterarStatusAulaRequestDTO { Status = "ativa" }));

            Assert.Equal("Preencha título, descrição, matéria e valor antes de publicar.", ex.Message);
            _aulaRepositoryMock.Verify(r => r.AtualizarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Fact]
        public async Task AlterarStatus_MesmoStatus_RetornaSemAtualizar()
        {
            AulaExiste(AulaCompleta("pausada"));

            var response = await _service.AlterarStatusAula(ProfessorId, AulaId, new AlterarStatusAulaRequestDTO { Status = "pausada" });

            Assert.Equal("pausada", response.Status);
            _aulaRepositoryMock.Verify(r => r.AtualizarAula(It.IsAny<Aula>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("arquivada")]
        public async Task AlterarStatus_StatusInvalido_LancaValidacao(string? status)
        {
            AulaExiste(AulaCompleta("ativa"));

            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.AlterarStatusAula(ProfessorId, AulaId, new AlterarStatusAulaRequestDTO { Status = status }));
        }

        [Fact]
        public async Task AlterarStatus_AulaInexistente_LancaNotFound()
        {
            _aulaRepositoryMock.Setup(r => r.GetAulaDoProfessor(AulaId, ProfessorId)).ReturnsAsync((Aula?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.AlterarStatusAula(ProfessorId, AulaId, new AlterarStatusAulaRequestDTO { Status = "ativa" }));
        }
    }
}
