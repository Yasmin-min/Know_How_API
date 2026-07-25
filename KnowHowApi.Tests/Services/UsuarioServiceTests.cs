using KnowHowApi.Domain.Configurations;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services;
using KnowHowApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
        private readonly Mock<ICryptography> _cryptographyMock = new();
        private readonly UsuarioService _service;

        public UsuarioServiceTests()
        {
            _cryptographyMock.Setup(c => c.Crypt(It.IsAny<string>())).Returns("hash-fake");

            var jwtSettings = new JWTSettings
            {
                Issuer = "KnowHowApi",
                Audience = "KnowHowApiClient",
                SecretKey = "chave-secreta-de-teste-com-tamanho-suficiente"
            };

            _service = new UsuarioService(_usuarioRepositoryMock.Object, _cryptographyMock.Object, jwtSettings);
        }

        private static RegisterUsuarioDTO NovoAlunoValido() => new()
        {
            Nome = "Aluno Teste",
            Email = "aluno.teste@knowhow.com",
            Senha = "Senha@123",
            TipoUsuario = TipoUsuario.Aluno,
            DataNascimento = new DateTime(2003, 5, 14)
        };

        private static RegisterUsuarioDTO NovoProfessorValido() => new()
        {
            Nome = "Professor Teste",
            Email = "professor.teste@knowhow.com",
            Senha = "Senha@123",
            TipoUsuario = TipoUsuario.Professor,
            DataNascimento = new DateTime(1988, 11, 2),
            Cpf = "71428793860"
        };

        [Fact]
        public async Task Registrar_AlunoComDadosValidos_PersisteComDataNascimentoESemCpf()
        {
            var request = NovoAlunoValido();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail(request.Email)).ReturnsAsync((Usuario?)null);
            _usuarioRepositoryMock.Setup(r => r.CriarUsuario(It.IsAny<Usuario>())).ReturnsAsync((Usuario u) => u);

            var usuario = await _service.Registrar(request);

            Assert.Equal(request.DataNascimento, usuario.DataNascimento);
            Assert.Null(usuario.Cpf);
            Assert.Equal(TipoUsuario.Aluno, usuario.TipoUsuario);
            _usuarioRepositoryMock.Verify(r => r.GetUsuarioByCpf(It.IsAny<string>()), Times.Never);
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task Registrar_ProfessorComCpfValido_PersisteComCpf()
        {
            var request = NovoProfessorValido();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail(request.Email)).ReturnsAsync((Usuario?)null);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf(request.Cpf!)).ReturnsAsync((Usuario?)null);
            _usuarioRepositoryMock.Setup(r => r.CriarUsuario(It.IsAny<Usuario>())).ReturnsAsync((Usuario u) => u);

            var usuario = await _service.Registrar(request);

            Assert.Equal(request.Cpf, usuario.Cpf);
            Assert.Equal(request.DataNascimento, usuario.DataNascimento);
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task Registrar_SemDataNascimento_LancaBadRequest()
        {
            var request = NovoAlunoValido();
            request.DataNascimento = default;

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Registrar_DataNascimentoNoFuturo_LancaBadRequest()
        {
            var request = NovoAlunoValido();
            request.DataNascimento = DateTime.UtcNow.AddDays(1);

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Registrar_ProfessorSemCpf_LancaBadRequest()
        {
            var request = NovoProfessorValido();
            request.Cpf = null;

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Theory]
        [InlineData("529.982.247-25")]
        [InlineData("5299822472")]
        [InlineData("529982247256")]
        [InlineData("abcdefghijk")]
        public async Task Registrar_ProfessorComCpfEmFormatoInvalido_LancaBadRequest(string cpfInvalido)
        {
            var request = NovoProfessorValido();
            request.Cpf = cpfInvalido;

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Registrar_ProfessorComCpfJaCadastrado_LancaBadRequest()
        {
            var request = NovoProfessorValido();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf(request.Cpf!))
                .ReturnsAsync(new Usuario { Id = 99, Cpf = request.Cpf });

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Registrar_EmailJaCadastrado_LancaBadRequest()
        {
            var request = NovoAlunoValido();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail(request.Email))
                .ReturnsAsync(new Usuario { Id = 1, Email = request.Email });

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task EditarUsuario_AtualizaDataNascimentoECpf()
        {
            var usuarioExistente = new Usuario
            {
                Id = 1,
                Nome = "Nome Antigo",
                Email = "antigo@knowhow.com",
                TipoUsuario = TipoUsuario.Professor,
                DataNascimento = new DateTime(1990, 1, 1),
                Cpf = "11111111111"
            };

            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(1)).ReturnsAsync(usuarioExistente);
            _usuarioRepositoryMock.Setup(r => r.Update(It.IsAny<Usuario>())).ReturnsAsync((Usuario u) => u);

            var edicao = new UsuarioDTO
            {
                Nome = "Nome Novo",
                Email = "novo@knowhow.com",
                TipoUsuario = TipoUsuario.Professor,
                DataNascimento = new DateTime(1991, 2, 3),
                Cpf = "22222222222"
            };

            var atualizado = await _service.EditarUsuario(1, edicao);

            Assert.Equal(edicao.DataNascimento, atualizado.DataNascimento);
            Assert.Equal(edicao.Cpf, atualizado.Cpf);
        }
    }
}
