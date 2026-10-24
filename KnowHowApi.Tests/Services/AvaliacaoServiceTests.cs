using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class AvaliacaoServiceTests
    {
        private const int ProfessorId = 10;
        private const int AlunoId = 12;

        private readonly Mock<IAvaliacaoRepository> _repositoryMock = new();
        private readonly AvaliacaoService _service;

        public AvaliacaoServiceTests()
        {
            _service = new AvaliacaoService(_repositoryMock.Object);
            _repositoryMock.Setup(r => r.ProfessorExiste(ProfessorId)).ReturnsAsync(true);
        }

        private static Avaliacao NovaAvaliacao(int id, int alunoId, int nota) => new()
        {
            Id = id,
            ProfessorId = ProfessorId,
            AlunoId = alunoId,
            Aluno = new Usuario { Id = alunoId, Nome = $"Aluno {alunoId}" },
            Nota = nota,
            Comentario = "",
            CriadaEm = DateTime.UtcNow
        };

        private void AlunoTeveAula(bool teve) =>
            _repositoryMock.Setup(r => r.AlunoTeveAulaComProfessor(ProfessorId, AlunoId, It.IsAny<DateTime>()))
                .ReturnsAsync(teve);

        [Fact]
        public async Task Listar_CalculaResumoEDistribuicao()
        {
            _repositoryMock.Setup(r => r.ListarPorProfessor(ProfessorId)).ReturnsAsync(new List<Avaliacao>
            {
                NovaAvaliacao(1, 20, 5),
                NovaAvaliacao(2, 21, 4),
                NovaAvaliacao(3, 22, 4)
            });

            var resultado = await _service.ListarDoProfessor(ProfessorId, null);

            Assert.Equal(3, resultado.Resumo.Total);
            Assert.Equal(4.3m, resultado.Resumo.Media);
            Assert.Equal(new[] { 0, 0, 0, 2, 1 }, resultado.Resumo.Distribuicao);
            Assert.Equal("Aluno 20", resultado.Avaliacoes[0].AlunoNome);
            Assert.Null(resultado.MinhaAvaliacao);
        }

        [Fact]
        public async Task Listar_AlunoLogado_PreencheMinhaAvaliacao()
        {
            _repositoryMock.Setup(r => r.ListarPorProfessor(ProfessorId)).ReturnsAsync(new List<Avaliacao>
            {
                NovaAvaliacao(1, 20, 5),
                NovaAvaliacao(2, AlunoId, 3)
            });

            var resultado = await _service.ListarDoProfessor(ProfessorId, AlunoId);

            Assert.Equal(2, resultado.MinhaAvaliacao?.Id);
        }

        [Fact]
        public async Task Listar_SemAvaliacoes_MediaZero()
        {
            _repositoryMock.Setup(r => r.ListarPorProfessor(ProfessorId)).ReturnsAsync(new List<Avaliacao>());

            var resultado = await _service.ListarDoProfessor(ProfessorId, AlunoId);

            Assert.Equal(0, resultado.Resumo.Total);
            Assert.Equal(0m, resultado.Resumo.Media);
            Assert.Null(resultado.MinhaAvaliacao);
        }

        [Fact]
        public async Task Listar_ProfessorInexistente_LancaNotFound()
        {
            _repositoryMock.Setup(r => r.ProfessorExiste(99)).ReturnsAsync(false);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.ListarDoProfessor(99, AlunoId));
        }

        [Fact]
        public async Task Salvar_ComAula_SalvaComentarioAparadoEAtualizaPerfil()
        {
            AlunoTeveAula(true);
            _repositoryMock.Setup(r => r.Salvar(ProfessorId, AlunoId, 5, "Ótimo professor", It.IsAny<DateTime>()))
                .ReturnsAsync(NovaAvaliacao(7, AlunoId, 5));

            var avaliacao = await _service.Salvar(AlunoId, ProfessorId,
                new SalvarAvaliacaoRequestDTO { Nota = 5, Comentario = "  Ótimo professor  " });

            Assert.Equal(7, avaliacao.Id);
            Assert.Equal($"Aluno {AlunoId}", avaliacao.AlunoNome);
            _repositoryMock.Verify(r => r.AtualizarResumoDoPerfil(ProfessorId), Times.Once);
        }

        [Fact]
        public async Task Salvar_SemAula_LancaForbidden()
        {
            AlunoTeveAula(false);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                _service.Salvar(AlunoId, ProfessorId, new SalvarAvaliacaoRequestDTO { Nota = 4 }));
            _repositoryMock.Verify(r => r.Salvar(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public async Task Salvar_NotaForaDaFaixa_LancaValidacao(int nota)
        {
            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.Salvar(AlunoId, ProfessorId, new SalvarAvaliacaoRequestDTO { Nota = nota }));
        }

        [Fact]
        public async Task Salvar_ComentarioLongo_LancaValidacao()
        {
            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.Salvar(AlunoId, ProfessorId, new SalvarAvaliacaoRequestDTO { Nota = 5, Comentario = new string('a', 501) }));
        }

        [Fact]
        public async Task Excluir_Existente_AtualizaPerfil()
        {
            _repositoryMock.Setup(r => r.Remover(ProfessorId, AlunoId)).ReturnsAsync(true);

            await _service.Excluir(AlunoId, ProfessorId);

            _repositoryMock.Verify(r => r.AtualizarResumoDoPerfil(ProfessorId), Times.Once);
        }

        [Fact]
        public async Task Excluir_Inexistente_LancaNotFound()
        {
            _repositoryMock.Setup(r => r.Remover(ProfessorId, AlunoId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.Excluir(AlunoId, ProfessorId));
        }
    }
}
