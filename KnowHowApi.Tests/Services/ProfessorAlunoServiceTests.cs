using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class ProfessorAlunoServiceTests
    {
        private const int ProfessorId = 10;
        private const int AlunoId = 12;

        private readonly Mock<IProfessorAlunoRepository> _repositoryMock = new();
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
        private readonly ProfessorAlunoService _service;

        public ProfessorAlunoServiceTests()
        {
            _service = new ProfessorAlunoService(_repositoryMock.Object, _usuarioRepositoryMock.Object);
        }

        private void AlunoNaLista() =>
            _repositoryMock.Setup(r => r.GetAlunoDoProfessor(ProfessorId, AlunoId))
                .ReturnsAsync(new Usuario { Id = AlunoId, Nome = "Diego Rocha" });

        private void AlunoForaDaLista() =>
            _repositoryMock.Setup(r => r.GetAlunoDoProfessor(ProfessorId, AlunoId)).ReturnsAsync((Usuario?)null);

        [Fact]
        public async Task ListarAlunos_MapeiaIdENome()
        {
            _repositoryMock.Setup(r => r.ListarAlunosDoProfessor(ProfessorId)).ReturnsAsync(new List<Usuario>
            {
                new() { Id = 12, Nome = "Ana Souza" },
                new() { Id = 15, Nome = "Bruno Lima" }
            });

            var alunos = await _service.ListarAlunos(ProfessorId);

            Assert.Equal(new[] { 12, 15 }, alunos.Select(a => a.Id));
            Assert.Equal("Ana Souza", alunos[0].Nome);
        }

        [Fact]
        public async Task AdicionarAluno_NormalizaEmailEVincula()
        {
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmailIgnorandoCaixa("aluno@teste.com"))
                .ReturnsAsync(new Usuario { Id = AlunoId, Nome = "Diego Rocha", TipoUsuario = TipoUsuario.Aluno });

            var aluno = await _service.AdicionarAlunoPorEmail(ProfessorId, new AdicionarAlunoRequestDTO { Email = "  Aluno@Teste.COM " });

            Assert.Equal(AlunoId, aluno.Id);
            Assert.Equal("Diego Rocha", aluno.Nome);
            _repositoryMock.Verify(r => r.AdicionarAluno(ProfessorId, AlunoId), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("sem-arroba")]
        [InlineData("aluno@semdominio")]
        [InlineData("dois espacos@teste.com")]
        public async Task AdicionarAluno_EmailVazioOuInvalido_LancaValidacao(string? email)
        {
            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.AdicionarAlunoPorEmail(ProfessorId, new AdicionarAlunoRequestDTO { Email = email }));
            _usuarioRepositoryMock.Verify(r => r.GetUsuarioByEmailIgnorandoCaixa(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AdicionarAluno_EmailSemUsuario_LancaNotFound()
        {
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmailIgnorandoCaixa("ninguem@teste.com")).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.AdicionarAlunoPorEmail(ProfessorId, new AdicionarAlunoRequestDTO { Email = "ninguem@teste.com" }));
        }

        [Fact]
        public async Task AdicionarAluno_UsuarioProfessor_LancaValidacao()
        {
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmailIgnorandoCaixa("prof@teste.com"))
                .ReturnsAsync(new Usuario { Id = 30, Nome = "Prof", TipoUsuario = TipoUsuario.Professor });

            var ex = await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.AdicionarAlunoPorEmail(ProfessorId, new AdicionarAlunoRequestDTO { Email = "prof@teste.com" }));

            Assert.Equal("Esse e-mail não pertence a um aluno.", ex.Message);
            _repositoryMock.Verify(r => r.AdicionarAluno(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ObterNota_SemAnotacao_RetornaTextoVazioEDataNula()
        {
            AlunoNaLista();
            _repositoryMock.Setup(r => r.GetNota(ProfessorId, AlunoId)).ReturnsAsync((NotaAluno?)null);

            var nota = await _service.ObterNota(ProfessorId, AlunoId);

            Assert.Equal("", nota.Texto);
            Assert.Null(nota.AtualizadaEm);
        }

        [Fact]
        public async Task ObterNota_ComAnotacao_RetornaTextoEData()
        {
            AlunoNaLista();
            var data = new DateTime(2026, 10, 13, 22, 40, 0, DateTimeKind.Utc);
            _repositoryMock.Setup(r => r.GetNota(ProfessorId, AlunoId)).ReturnsAsync(new NotaAluno { Texto = "Revisar useEffect.", AtualizadaEm = data });

            var nota = await _service.ObterNota(ProfessorId, AlunoId);

            Assert.Equal("Revisar useEffect.", nota.Texto);
            Assert.Equal(data, nota.AtualizadaEm);
        }

        [Fact]
        public async Task ObterNota_AlunoForaDaLista_LancaNotFound()
        {
            AlunoForaDaLista();

            await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterNota(ProfessorId, AlunoId));
            _repositoryMock.Verify(r => r.GetNota(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task SalvarNota_FazTrimESalva()
        {
            AlunoNaLista();
            _repositoryMock.Setup(r => r.SalvarNota(ProfessorId, AlunoId, "Dificuldade com hooks.", It.IsAny<DateTime>()))
                .ReturnsAsync((int p, int a, string t, DateTime d) => new NotaAluno { Texto = t, AtualizadaEm = d });

            var nota = await _service.SalvarNota(ProfessorId, AlunoId, new SalvarNotaAlunoRequestDTO { Texto = "  Dificuldade com hooks.  " });

            Assert.Equal("Dificuldade com hooks.", nota.Texto);
            Assert.NotNull(nota.AtualizadaEm);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task SalvarNota_TextoVazio_ApagaAnotacao(string? texto)
        {
            AlunoNaLista();

            var nota = await _service.SalvarNota(ProfessorId, AlunoId, new SalvarNotaAlunoRequestDTO { Texto = texto });

            Assert.Equal("", nota.Texto);
            Assert.Null(nota.AtualizadaEm);
            _repositoryMock.Verify(r => r.RemoverNota(ProfessorId, AlunoId), Times.Once);
            _repositoryMock.Verify(r => r.SalvarNota(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task SalvarNota_AcimaDe2000Caracteres_LancaValidacao()
        {
            AlunoNaLista();

            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.SalvarNota(ProfessorId, AlunoId, new SalvarNotaAlunoRequestDTO { Texto = new string('a', 2001) }));
        }

        [Fact]
        public async Task SalvarNota_AlunoForaDaLista_LancaNotFound()
        {
            AlunoForaDaLista();

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.SalvarNota(ProfessorId, AlunoId, new SalvarNotaAlunoRequestDTO { Texto = "x" }));
        }
    }
}
