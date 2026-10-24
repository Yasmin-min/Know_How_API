using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class AulaAgendadaServiceTests
    {
        private const int ProfessorId = 10;
        private const int AlunoId = 12;

        private readonly Mock<IAulaAgendadaRepository> _repositoryMock = new();
        private readonly Mock<IProfessorAlunoRepository> _professorAlunoRepositoryMock = new();
        private readonly Mock<IAulaRepository> _aulaRepositoryMock = new();
        private readonly AulaAgendadaService _service;

        public AulaAgendadaServiceTests()
        {
            _service = new AulaAgendadaService(_repositoryMock.Object, _professorAlunoRepositoryMock.Object, _aulaRepositoryMock.Object);
        }

        private static DateTime AmanhaAs17() => DateTime.UtcNow.Date.AddDays(1).AddHours(17);

        private static CriarAulaAgendadaRequestDTO RequestValido() => new()
        {
            Titulo = "Revisão de funções do 2º grau",
            Descricao = "Trazer a lista de exercícios 3.",
            AlunoId = AlunoId,
            Inicio = AmanhaAs17(),
            DuracaoMinutos = 90
        };

        private void AlunoPertenceAoProfessor()
        {
            _professorAlunoRepositoryMock.Setup(r => r.GetAlunoDoProfessor(ProfessorId, AlunoId))
                .ReturnsAsync(new Usuario { Id = AlunoId, Nome = "Ana Souza" });
            _repositoryMock.Setup(r => r.Criar(It.IsAny<AulaAgendada>()))
                .ReturnsAsync((AulaAgendada a) => a);
        }

        private static AulaAgendada AulaExistente(DateTime inicio, int duracao = 60, string status = AulaAgendada.StatusAgendada) => new()
        {
            Id = Guid.NewGuid(),
            ProfessorId = ProfessorId,
            AlunoId = AlunoId,
            Aluno = new Usuario { Id = AlunoId, Nome = "Ana Souza" },
            Titulo = "Aula existente",
            Descricao = "",
            Inicio = inicio,
            Fim = inicio.AddMinutes(duracao),
            DuracaoMinutos = duracao,
            Status = status,
            CriadaEm = DateTime.UtcNow.AddDays(-1)
        };

        [Fact]
        public async Task ListarAulas_ConverteParametrosLocaisParaUtc()
        {
            var inicioUtc = new DateTime(2026, 10, 12, 3, 0, 0, DateTimeKind.Utc);
            var fimUtc = inicioUtc.AddDays(7);
            _repositoryMock.Setup(r => r.ListarDoProfessor(ProfessorId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .ReturnsAsync(new List<AulaAgendada>());

            await _service.ListarAulas(ProfessorId, inicioUtc.ToLocalTime(), fimUtc.ToLocalTime());

            _repositoryMock.Verify(r => r.ListarDoProfessor(ProfessorId,
                It.Is<DateTime?>(d => d!.Value.Kind == DateTimeKind.Utc && d.Value == inicioUtc),
                It.Is<DateTime?>(d => d!.Value.Kind == DateTimeKind.Utc && d.Value == fimUtc)), Times.Once);
        }

        [Fact]
        public async Task ListarAulas_SemParametros_RepassaNulos()
        {
            var aula = AulaExistente(AmanhaAs17(), status: AulaAgendada.StatusCancelada);
            _repositoryMock.Setup(r => r.ListarDoProfessor(ProfessorId, null, null))
                .ReturnsAsync(new List<AulaAgendada> { aula });

            var aulas = await _service.ListarAulas(ProfessorId, null, null);

            var dto = Assert.Single(aulas);
            Assert.Equal(aula.Id.ToString(), dto.Id);
            Assert.Equal("cancelada", dto.Status);
            Assert.Equal("Ana Souza", dto.AlunoNome);
            Assert.Equal("", dto.Descricao);
        }

        [Fact]
        public async Task ListarAulas_FimAntesDoInicio_LancaValidacao()
        {
            var inicio = DateTime.UtcNow;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.ListarAulas(ProfessorId, inicio, inicio.AddHours(-1)));
        }

        [Fact]
        public async Task CriarAula_Valida_RetornaAgendadaComNomeDoAlunoEFimCalculado()
        {
            AlunoPertenceAoProfessor();
            var request = RequestValido();
            request.Descricao = null;

            var dto = await _service.CriarAula(ProfessorId, request);

            Assert.Equal("agendada", dto.Status);
            Assert.Equal("Ana Souza", dto.AlunoNome);
            Assert.Equal("", dto.Descricao);
            Assert.Equal(AlunoId, dto.AlunoId);
            Assert.Equal(DateTimeKind.Utc, dto.Inicio.Kind);
            Assert.True(Guid.TryParse(dto.Id, out _));
            _repositoryMock.Verify(r => r.Criar(It.Is<AulaAgendada>(a =>
                a.ProfessorId == ProfessorId && a.Fim == a.Inicio.AddMinutes(90))), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task CriarAula_SemTitulo_LancaValidacao(string? titulo)
        {
            var request = RequestValido();
            request.Titulo = titulo;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(ProfessorId, request));
        }

        [Fact]
        public async Task CriarAula_TituloLongo_LancaValidacao()
        {
            var request = RequestValido();
            request.Titulo = new string('a', 101);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(ProfessorId, request));
        }

        [Fact]
        public async Task CriarAula_DescricaoLonga_LancaValidacao()
        {
            var request = RequestValido();
            request.Descricao = new string('a', 501);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(ProfessorId, request));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(14)]
        [InlineData(481)]
        public async Task CriarAula_DuracaoForaDoIntervalo_LancaValidacao(int? duracao)
        {
            var request = RequestValido();
            request.DuracaoMinutos = duracao;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(ProfessorId, request));
        }

        [Fact]
        public async Task CriarAula_InicioNoPassado_LancaValidacao()
        {
            AlunoPertenceAoProfessor();
            var request = RequestValido();
            request.Inicio = DateTime.UtcNow.AddMinutes(-5);

            var ex = await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(ProfessorId, request));
            Assert.Equal("Não é possível agendar uma aula no passado.", ex.Message);
        }

        [Fact]
        public async Task CriarAula_AlunoQueNaoEhDoProfessor_LancaValidacao()
        {
            _professorAlunoRepositoryMock.Setup(r => r.GetAlunoDoProfessor(ProfessorId, AlunoId)).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(ProfessorId, RequestValido()));
            _repositoryMock.Verify(r => r.Criar(It.IsAny<AulaAgendada>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_ComAulaDoCatalogo_GuardaEDevolveAulaId()
        {
            AlunoPertenceAoProfessor();
            _aulaRepositoryMock.Setup(r => r.GetAulaDoProfessor(31, ProfessorId)).ReturnsAsync(new Aula { Id = 31, ProfessorId = ProfessorId });
            var request = RequestValido();
            request.AulaId = 31;

            var dto = await _service.CriarAula(ProfessorId, request);

            Assert.Equal(31, dto.AulaId);
            _repositoryMock.Verify(r => r.Criar(It.Is<AulaAgendada>(a => a.AulaId == 31)), Times.Once);
        }

        [Fact]
        public async Task CriarAula_AulaDeOutroProfessorOuInexistente_LancaValidacao()
        {
            AlunoPertenceAoProfessor();
            _aulaRepositoryMock.Setup(r => r.GetAulaDoProfessor(99, ProfessorId)).ReturnsAsync((Aula?)null);
            var request = RequestValido();
            request.AulaId = 99;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarAula(ProfessorId, request));
            _repositoryMock.Verify(r => r.Criar(It.IsAny<AulaAgendada>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_SemAulaId_GravaNulo()
        {
            AlunoPertenceAoProfessor();

            var dto = await _service.CriarAula(ProfessorId, RequestValido());

            Assert.Null(dto.AulaId);
            _aulaRepositoryMock.Verify(r => r.GetAulaDoProfessor(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_ConflitoNaAgendaDoProfessor_LancaConflitoComTituloEAluno()
        {
            AlunoPertenceAoProfessor();
            var existente = AulaExistente(AmanhaAs17().AddMinutes(30));
            _repositoryMock.Setup(r => r.GetConflitoProfessor(ProfessorId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>()))
                .ReturnsAsync(existente);

            var ex = await Assert.ThrowsAsync<ConflictException>(() => _service.CriarAula(ProfessorId, RequestValido()));

            Assert.Equal("Já existe uma aula neste horário: \"Aula existente\" com Ana Souza.", ex.Message);
            _repositoryMock.Verify(r => r.Criar(It.IsAny<AulaAgendada>()), Times.Never);
        }

        [Fact]
        public async Task CriarAula_ConflitoNaAgendaDoAluno_LancaConflito()
        {
            AlunoPertenceAoProfessor();
            _repositoryMock.Setup(r => r.ExisteConflitoAluno(AlunoId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>()))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<ConflictException>(() => _service.CriarAula(ProfessorId, RequestValido()));
            _repositoryMock.Verify(r => r.Criar(It.IsAny<AulaAgendada>()), Times.Never);
        }

        private AulaAgendada AulaDoProfessorFutura()
        {
            var aula = AulaExistente(AmanhaAs17());
            _repositoryMock.Setup(r => r.GetByIdDoProfessor(aula.Id, ProfessorId)).ReturnsAsync(aula);
            return aula;
        }

        private static EditarAulaAgendadaRequestDTO EdicaoValida(DateTime inicio) => new()
        {
            Titulo = "  Novo título  ",
            Descricao = "Nova descrição",
            Inicio = inicio,
            DuracaoMinutos = 45
        };

        [Fact]
        public async Task EditarAula_Valida_AtualizaIgnorandoAPropriaAulaNosConflitos()
        {
            var aula = AulaDoProfessorFutura();
            var novoInicio = AmanhaAs17().AddHours(2);

            var resultado = await _service.EditarAula(ProfessorId, aula.Id, EdicaoValida(novoInicio));

            Assert.Equal("Novo título", resultado.Titulo);
            Assert.Equal(novoInicio, resultado.Inicio);
            Assert.Equal(45, resultado.DuracaoMinutos);
            _repositoryMock.Verify(r => r.GetConflitoProfessor(ProfessorId, novoInicio, novoInicio.AddMinutes(45), aula.Id), Times.Once);
            _repositoryMock.Verify(r => r.ExisteConflitoAluno(AlunoId, novoInicio, novoInicio.AddMinutes(45), aula.Id), Times.Once);
            _repositoryMock.Verify(r => r.Atualizar(aula.Id, null, "Novo título", "Nova descrição", novoInicio, novoInicio.AddMinutes(45), 45), Times.Once);
        }

        [Fact]
        public async Task EditarAula_Inexistente_LancaNotFound()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdDoProfessor(id, ProfessorId)).ReturnsAsync((AulaAgendada?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.EditarAula(ProfessorId, id, EdicaoValida(AmanhaAs17())));
        }

        [Fact]
        public async Task EditarAula_Cancelada_LancaValidacao()
        {
            var aula = AulaExistente(AmanhaAs17(), status: AulaAgendada.StatusCancelada);
            _repositoryMock.Setup(r => r.GetByIdDoProfessor(aula.Id, ProfessorId)).ReturnsAsync(aula);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.EditarAula(ProfessorId, aula.Id, EdicaoValida(AmanhaAs17())));
            _repositoryMock.Verify(r => r.Atualizar(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task EditarAula_JaAconteceu_LancaValidacao()
        {
            var aula = AulaExistente(DateTime.UtcNow.AddDays(-2));
            _repositoryMock.Setup(r => r.GetByIdDoProfessor(aula.Id, ProfessorId)).ReturnsAsync(aula);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.EditarAula(ProfessorId, aula.Id, EdicaoValida(AmanhaAs17())));
        }

        [Fact]
        public async Task EditarAula_RemarcarParaOPassado_LancaValidacao()
        {
            var aula = AulaDoProfessorFutura();

            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.EditarAula(ProfessorId, aula.Id, EdicaoValida(DateTime.UtcNow.AddDays(-1))));
        }

        [Fact]
        public async Task EditarAula_EmAndamentoMantendoHorario_PermiteMudarTitulo()
        {
            var inicio = DateTime.UtcNow.AddMinutes(-10);
            var aula = AulaExistente(inicio, 60);
            _repositoryMock.Setup(r => r.GetByIdDoProfessor(aula.Id, ProfessorId)).ReturnsAsync(aula);

            var resultado = await _service.EditarAula(ProfessorId, aula.Id, new EditarAulaAgendadaRequestDTO
            {
                Titulo = "Título corrigido",
                Descricao = "",
                Inicio = inicio,
                DuracaoMinutos = 60
            });

            Assert.Equal("Título corrigido", resultado.Titulo);
        }

        [Fact]
        public async Task EditarAula_ConflitoComOutraAulaDoProfessor_LancaConflito()
        {
            var aula = AulaDoProfessorFutura();
            _repositoryMock.Setup(r => r.GetConflitoProfessor(ProfessorId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), aula.Id))
                .ReturnsAsync(AulaExistente(AmanhaAs17()));

            await Assert.ThrowsAsync<ConflictException>(() => _service.EditarAula(ProfessorId, aula.Id, EdicaoValida(AmanhaAs17())));
            _repositoryMock.Verify(r => r.Atualizar(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task EditarAula_ConflitoComOutraAulaDoAluno_LancaConflito()
        {
            var aula = AulaDoProfessorFutura();
            _repositoryMock.Setup(r => r.ExisteConflitoAluno(AlunoId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), aula.Id))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<ConflictException>(() => _service.EditarAula(ProfessorId, aula.Id, EdicaoValida(AmanhaAs17())));
        }

        [Fact]
        public async Task CancelarAula_Inexistente_LancaNotFound()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdDoProfessor(id, ProfessorId)).ReturnsAsync((AulaAgendada?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.CancelarAula(ProfessorId, id));
        }

        [Fact]
        public async Task CancelarAula_Futura_MudaStatusParaCancelada()
        {
            var aula = AulaExistente(AmanhaAs17());
            _repositoryMock.Setup(r => r.GetByIdDoProfessor(aula.Id, ProfessorId)).ReturnsAsync(aula);

            var dto = await _service.CancelarAula(ProfessorId, aula.Id);

            Assert.Equal("cancelada", dto.Status);
            _repositoryMock.Verify(r => r.Cancelar(aula.Id, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task CancelarAula_JaCancelada_RetornaSemEfeito()
        {
            var aula = AulaExistente(AmanhaAs17(), status: AulaAgendada.StatusCancelada);
            _repositoryMock.Setup(r => r.GetByIdDoProfessor(aula.Id, ProfessorId)).ReturnsAsync(aula);

            var dto = await _service.CancelarAula(ProfessorId, aula.Id);

            Assert.Equal("cancelada", dto.Status);
            _repositoryMock.Verify(r => r.Cancelar(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task CancelarAula_JaTerminou_LancaValidacao()
        {
            var aula = AulaExistente(DateTime.UtcNow.AddHours(-3), duracao: 60);
            _repositoryMock.Setup(r => r.GetByIdDoProfessor(aula.Id, ProfessorId)).ReturnsAsync(aula);

            var ex = await Assert.ThrowsAsync<DomainValidationException>(() => _service.CancelarAula(ProfessorId, aula.Id));

            Assert.Equal("Não é possível cancelar uma aula que já aconteceu.", ex.Message);
        }

        [Fact]
        public async Task ListarAulasDoAluno_MapeiaProfessor()
        {
            var inicio = AmanhaAs17();
            _repositoryMock.Setup(r => r.ListarDoAluno(AlunoId, null, null)).ReturnsAsync(new List<AulaAgendada>
            {
                new()
                {
                    Id = Guid.NewGuid(), ProfessorId = ProfessorId, Professor = new Usuario { Id = ProfessorId, Nome = "Marina Costa" },
                    AlunoId = AlunoId, Titulo = "Inglês", Descricao = "", Inicio = inicio, Fim = inicio.AddMinutes(60),
                    DuracaoMinutos = 60, Status = AulaAgendada.StatusAgendada
                }
            });

            var aulas = await _service.ListarAulasDoAluno(AlunoId, null, null);

            var aula = Assert.Single(aulas);
            Assert.Equal(ProfessorId, aula.ProfessorId);
            Assert.Equal("Marina Costa", aula.ProfessorNome);
            Assert.Equal(60, aula.DuracaoMinutos);
        }

        [Fact]
        public async Task ListarAulasDoAluno_PeriodoInvertido_LancaValidacao()
        {
            var inicio = AmanhaAs17();
            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.ListarAulasDoAluno(AlunoId, inicio, inicio.AddHours(-1)));
        }
    }
}
