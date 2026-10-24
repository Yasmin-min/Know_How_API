using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class PlanosServiceTests
    {
        private const int ProfessorId = 10;
        private const int AlunoId = 12;

        private readonly Mock<IPlanosRepository> _repositoryMock = new();
        private readonly Mock<IProfessorRepository> _professorRepositoryMock = new();
        private readonly Mock<IProfessorAlunoRepository> _professorAlunoRepositoryMock = new();
        private readonly PlanosService _service;

        public PlanosServiceTests()
        {
            _service = new PlanosService(_repositoryMock.Object, _professorRepositoryMock.Object, _professorAlunoRepositoryMock.Object);
        }

        private void PerfilExiste(decimal valorHora = 80m)
        {
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(ProfessorId))
                .ReturnsAsync(new PerfilProfessor { UsuarioId = ProfessorId, ValorHora = valorHora });
        }

        private static AtualizarPrecosRequestDTO PrecosValidos() => new()
        {
            ValorHora = 90m,
            PrecosPorDuracao = new List<PrecoPorDuracaoDTO>
            {
                new() { DuracaoMinutos = 90, Valor = 120m },
                new() { DuracaoMinutos = 30, Valor = 50m }
            },
            AulaExperimental = new AulaExperimentalDTO { Tipo = "desconto", PercentualDesconto = 50, DuracaoMinutos = 30 }
        };

        private static SalvarPacoteRequestDTO PacoteValido() => new()
        {
            Nome = "  Pacote 8 aulas ",
            Descricao = "Ideal para o vestibular",
            QuantidadeAulas = 8,
            DuracaoMinutos = 60,
            ValorTotal = 560m,
            ValidadeDias = 60,
            Ativo = true
        };

        private static SalvarPlanoMensalRequestDTO PlanoValido() => new()
        {
            Nome = "2x por semana",
            Descricao = "",
            AulasPorSemana = 2,
            DuracaoMinutos = 60,
            ValorMensal = 600m,
            Ativo = true
        };

        private static Contratacao ContratacaoExistente(string status = Contratacao.StatusPendente, int totalAulas = 8, int aulasUsadas = 2) => new()
        {
            Id = Guid.NewGuid(),
            ProfessorId = ProfessorId,
            AlunoId = AlunoId,
            Aluno = new Usuario { Id = AlunoId, Nome = "Ana Souza" },
            Tipo = Contratacao.TipoPacote,
            OfertaId = Guid.NewGuid(),
            Descricao = "Pacote 8 aulas",
            Valor = 560m,
            TotalAulas = totalAulas,
            AulasUsadas = aulasUsadas,
            Inicio = new DateOnly(2026, 10, 1),
            Fim = new DateOnly(2026, 11, 29),
            StatusPagamento = status,
            PagoEm = status == Contratacao.StatusPago ? new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc) : null,
            CriadaEm = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        // Preços

        [Fact]
        public async Task ObterPrecos_SemConfiguracao_RetornaPadraoComValorHoraDoPerfil()
        {
            PerfilExiste(80m);

            var dto = await _service.ObterPrecos(ProfessorId);

            Assert.Equal(80m, dto.ValorHora);
            Assert.Empty(dto.PrecosPorDuracao);
            Assert.Equal("nenhuma", dto.AulaExperimental.Tipo);
            Assert.Null(dto.AulaExperimental.PercentualDesconto);
            Assert.Equal(30, dto.AulaExperimental.DuracaoMinutos);
        }

        [Fact]
        public async Task ObterPrecos_ValorHoraZero_RetornaNulo()
        {
            PerfilExiste(0m);

            var dto = await _service.ObterPrecos(ProfessorId);

            Assert.Null(dto.ValorHora);
        }

        [Fact]
        public async Task ObterPrecos_SemPerfil_LancaNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterPrecos(ProfessorId));
        }

        [Fact]
        public async Task AtualizarPrecos_Valido_SalvaConfiguracaoEValorHoraJuntos()
        {
            PerfilExiste();

            var dto = await _service.AtualizarPrecos(ProfessorId, PrecosValidos());

            Assert.Equal(90m, dto.ValorHora);
            Assert.Equal(new[] { 30, 90 }, dto.PrecosPorDuracao.Select(p => p.DuracaoMinutos!.Value));
            Assert.Equal(50, dto.AulaExperimental.PercentualDesconto);
            _repositoryMock.Verify(r => r.SalvarConfiguracaoPrecos(It.Is<ConfiguracaoPrecos>(c =>
                c.ProfessorId == ProfessorId && c.AulaExperimentalTipo == "desconto" && c.PrecosPorDuracao.Count == 2), 90m), Times.Once);
        }

        [Fact]
        public async Task AtualizarPrecos_SemDesconto_DescartaPercentual()
        {
            PerfilExiste();
            var request = PrecosValidos();
            request.AulaExperimental!.Tipo = "gratis";

            var dto = await _service.AtualizarPrecos(ProfessorId, request);

            Assert.Null(dto.AulaExperimental.PercentualDesconto);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(100)]
        public async Task AtualizarPrecos_DescontoInvalido_LancaValidacao(int? percentual)
        {
            PerfilExiste();
            var request = PrecosValidos();
            request.AulaExperimental!.PercentualDesconto = percentual;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPrecos(ProfessorId, request));
        }

        [Fact]
        public async Task AtualizarPrecos_DuracaoRepetida_LancaValidacao()
        {
            PerfilExiste();
            var request = PrecosValidos();
            request.PrecosPorDuracao!.Add(new PrecoPorDuracaoDTO { DuracaoMinutos = 30, Valor = 40m });

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPrecos(ProfessorId, request));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public async Task AtualizarPrecos_ValorHoraInvalido_LancaValidacao(int? valorHora)
        {
            var request = PrecosValidos();
            request.ValorHora = valorHora;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPrecos(ProfessorId, request));
            _repositoryMock.Verify(r => r.SalvarConfiguracaoPrecos(It.IsAny<ConfiguracaoPrecos>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarPrecos_TipoExperimentalInvalido_LancaValidacao()
        {
            var request = PrecosValidos();
            request.AulaExperimental!.Tipo = "metade";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPrecos(ProfessorId, request));
        }

        // Pacotes e planos

        [Fact]
        public async Task CriarPacote_Valido_NormalizaERetornaComId()
        {
            _repositoryMock.Setup(r => r.CriarPacote(It.IsAny<Pacote>())).ReturnsAsync((Pacote p) => p);

            var dto = await _service.CriarPacote(ProfessorId, PacoteValido());

            Assert.Equal("Pacote 8 aulas", dto.Nome);
            Assert.True(Guid.TryParse(dto.Id, out _));
            _repositoryMock.Verify(r => r.CriarPacote(It.Is<Pacote>(p => p.ProfessorId == ProfessorId)), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(51)]
        public async Task CriarPacote_QuantidadeForaDoLimite_LancaValidacao(int quantidade)
        {
            var request = PacoteValido();
            request.QuantidadeAulas = quantidade;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarPacote(ProfessorId, request));
        }

        [Fact]
        public async Task CriarPacote_DuracaoInvalida_LancaValidacao()
        {
            var request = PacoteValido();
            request.DuracaoMinutos = 50;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarPacote(ProfessorId, request));
        }

        [Fact]
        public async Task AtualizarPacote_DeOutroProfessor_LancaNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.AtualizarPacote(ProfessorId, Guid.NewGuid(), PacoteValido()));
        }

        [Fact]
        public async Task AtualizarPacote_PreservaIdECriadoEm()
        {
            var existente = new Pacote { Id = Guid.NewGuid(), ProfessorId = ProfessorId, CriadoEm = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
            _repositoryMock.Setup(r => r.GetPacote(existente.Id, ProfessorId)).ReturnsAsync(existente);
            _repositoryMock.Setup(r => r.AtualizarPacote(It.IsAny<Pacote>())).ReturnsAsync((Pacote p) => p);
            var request = PacoteValido();
            request.Ativo = false;

            var dto = await _service.AtualizarPacote(ProfessorId, existente.Id, request);

            Assert.Equal(existente.Id.ToString(), dto.Id);
            Assert.Equal(existente.CriadoEm, dto.CriadoEm);
            Assert.False(dto.Ativo);
        }

        [Fact]
        public async Task ExcluirPacote_Inexistente_LancaNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.ExcluirPacote(ProfessorId, Guid.NewGuid()));
        }

        [Fact]
        public async Task CriarPlanoMensal_NomeLongo_LancaValidacao()
        {
            var request = PlanoValido();
            request.Nome = new string('a', 61);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarPlanoMensal(ProfessorId, request));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(8)]
        public async Task CriarPlanoMensal_AulasPorSemanaInvalidas_LancaValidacao(int aulasPorSemana)
        {
            var request = PlanoValido();
            request.AulasPorSemana = aulasPorSemana;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarPlanoMensal(ProfessorId, request));
        }

        // Ofertas

        [Fact]
        public async Task ObterOfertas_RetornaSomenteAtivos()
        {
            PerfilExiste(100m);
            _repositoryMock.Setup(r => r.ListarPacotes(ProfessorId, true))
                .ReturnsAsync(new List<Pacote> { new() { Id = Guid.NewGuid(), Nome = "Pacote", Ativo = true } });
            _repositoryMock.Setup(r => r.ListarPlanosMensais(ProfessorId, true))
                .ReturnsAsync(new List<PlanoMensal>());

            var dto = await _service.ObterOfertas(ProfessorId);

            Assert.Equal(100m, dto.Precos.ValorHora);
            Assert.Single(dto.Pacotes);
            Assert.Empty(dto.PlanosMensais);
            _repositoryMock.Verify(r => r.ListarPacotes(ProfessorId, false), Times.Never);
        }

        [Fact]
        public async Task ObterOfertas_ProfessorInexistente_LancaNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterOfertas(ProfessorId));
        }

        // Contratações

        private CriarContratacaoRequestDTO ContratacaoPacoteValida()
        {
            var pacoteId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetPacote(pacoteId, ProfessorId)).ReturnsAsync(new Pacote { Id = pacoteId, ProfessorId = ProfessorId });
            _professorAlunoRepositoryMock.Setup(r => r.GetAlunoDoProfessor(ProfessorId, AlunoId))
                .ReturnsAsync(new Usuario { Id = AlunoId, Nome = "Ana Souza" });
            _repositoryMock.Setup(r => r.CriarContratacao(It.IsAny<Contratacao>())).ReturnsAsync((Contratacao c) => c);

            return new CriarContratacaoRequestDTO
            {
                AlunoId = AlunoId,
                Tipo = "pacote",
                OfertaId = pacoteId.ToString(),
                Descricao = "Pacote 8 aulas",
                Valor = 560m,
                TotalAulas = 8,
                Inicio = new DateOnly(2026, 10, 14),
                Fim = new DateOnly(2026, 12, 12),
                Pago = true
            };
        }

        [Fact]
        public async Task CriarContratacao_PacotePago_PreencheNomeEPagamento()
        {
            var request = ContratacaoPacoteValida();

            var dto = await _service.CriarContratacao(ProfessorId, request);

            Assert.Equal("Ana Souza", dto.AlunoNome);
            Assert.Equal("pago", dto.StatusPagamento);
            Assert.NotNull(dto.PagoEm);
            Assert.Equal(0, dto.AulasUsadas);
            Assert.Equal(request.OfertaId, dto.OfertaId);
        }

        [Fact]
        public async Task CriarContratacao_Avulsa_SemOfertaEFim()
        {
            var request = ContratacaoPacoteValida();
            request.Tipo = "avulsa";
            request.OfertaId = null;
            request.Fim = null;
            request.Pago = false;

            var dto = await _service.CriarContratacao(ProfessorId, request);

            Assert.Null(dto.OfertaId);
            Assert.Null(dto.Fim);
            Assert.Equal("pendente", dto.StatusPagamento);
            Assert.Null(dto.PagoEm);
        }

        [Fact]
        public async Task CriarContratacao_AvulsaComOferta_LancaValidacao()
        {
            var request = ContratacaoPacoteValida();
            request.Tipo = "avulsa";
            request.Fim = null;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarContratacao(ProfessorId, request));
        }

        [Fact]
        public async Task CriarContratacao_PacoteDeOutroProfessor_LancaValidacao()
        {
            var request = ContratacaoPacoteValida();
            request.OfertaId = Guid.NewGuid().ToString();

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarContratacao(ProfessorId, request));
            _repositoryMock.Verify(r => r.CriarContratacao(It.IsAny<Contratacao>()), Times.Never);
        }

        [Fact]
        public async Task CriarContratacao_MensalComIdDePacote_LancaValidacao()
        {
            var request = ContratacaoPacoteValida();
            request.Tipo = "mensal";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarContratacao(ProfessorId, request));
        }

        [Fact]
        public async Task CriarContratacao_AlunoDeOutroProfessor_LancaValidacao()
        {
            var request = ContratacaoPacoteValida();
            request.AlunoId = 99;

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarContratacao(ProfessorId, request));
        }

        [Fact]
        public async Task CriarContratacao_FimAntesDoInicio_LancaValidacao()
        {
            var request = ContratacaoPacoteValida();
            request.Fim = request.Inicio!.Value.AddDays(-1);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.CriarContratacao(ProfessorId, request));
        }

        [Fact]
        public async Task AlterarPagamento_MarcarPago_PreencheData()
        {
            var contratacao = ContratacaoExistente();
            _repositoryMock.Setup(r => r.GetContratacao(contratacao.Id, ProfessorId)).ReturnsAsync(contratacao);
            _repositoryMock.Setup(r => r.AtualizarContratacao(It.IsAny<Contratacao>())).ReturnsAsync((Contratacao c) => c);

            var dto = await _service.AlterarPagamento(ProfessorId, contratacao.Id, new AlterarPagamentoContratacaoRequestDTO { Pago = true });

            Assert.Equal("pago", dto.StatusPagamento);
            Assert.NotNull(dto.PagoEm);
        }

        [Fact]
        public async Task AlterarPagamento_JaPaga_MantemDataOriginal()
        {
            var contratacao = ContratacaoExistente(Contratacao.StatusPago);
            var pagoEmOriginal = contratacao.PagoEm;
            _repositoryMock.Setup(r => r.GetContratacao(contratacao.Id, ProfessorId)).ReturnsAsync(contratacao);

            var dto = await _service.AlterarPagamento(ProfessorId, contratacao.Id, new AlterarPagamentoContratacaoRequestDTO { Pago = true });

            Assert.Equal(pagoEmOriginal, dto.PagoEm);
            _repositoryMock.Verify(r => r.AtualizarContratacao(It.IsAny<Contratacao>()), Times.Never);
        }

        [Fact]
        public async Task AlterarPagamento_Desmarcar_LimpaData()
        {
            var contratacao = ContratacaoExistente(Contratacao.StatusPago);
            _repositoryMock.Setup(r => r.GetContratacao(contratacao.Id, ProfessorId)).ReturnsAsync(contratacao);
            _repositoryMock.Setup(r => r.AtualizarContratacao(It.IsAny<Contratacao>())).ReturnsAsync((Contratacao c) => c);

            var dto = await _service.AlterarPagamento(ProfessorId, contratacao.Id, new AlterarPagamentoContratacaoRequestDTO { Pago = false });

            Assert.Equal("pendente", dto.StatusPagamento);
            Assert.Null(dto.PagoEm);
        }

        [Fact]
        public async Task AlterarPagamento_Inexistente_LancaNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.AlterarPagamento(ProfessorId, Guid.NewGuid(), new AlterarPagamentoContratacaoRequestDTO { Pago = true }));
        }

        [Fact]
        public async Task AlterarAulasUsadas_Valido_Atualiza()
        {
            var contratacao = ContratacaoExistente();
            _repositoryMock.Setup(r => r.GetContratacao(contratacao.Id, ProfessorId)).ReturnsAsync(contratacao);
            _repositoryMock.Setup(r => r.AtualizarContratacao(It.IsAny<Contratacao>())).ReturnsAsync((Contratacao c) => c);

            var dto = await _service.AlterarAulasUsadas(ProfessorId, contratacao.Id, new AlterarAulasUsadasRequestDTO { AulasUsadas = 8 });

            Assert.Equal(8, dto.AulasUsadas);
            Assert.Equal("Ana Souza", dto.AlunoNome);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(9)]
        public async Task AlterarAulasUsadas_ForaDoLimite_LancaValidacao(int aulasUsadas)
        {
            var contratacao = ContratacaoExistente();
            _repositoryMock.Setup(r => r.GetContratacao(contratacao.Id, ProfessorId)).ReturnsAsync(contratacao);

            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.AlterarAulasUsadas(ProfessorId, contratacao.Id, new AlterarAulasUsadasRequestDTO { AulasUsadas = aulasUsadas }));
        }

        [Fact]
        public async Task ExcluirContratacao_Inexistente_LancaNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.ExcluirContratacao(ProfessorId, Guid.NewGuid()));
        }
    }
}
