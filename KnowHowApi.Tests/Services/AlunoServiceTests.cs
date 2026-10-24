using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class AlunoServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
        private readonly Mock<IAreaInteresseRepository> _areaInteresseRepositoryMock = new();
        private readonly AlunoService _service;

        public AlunoServiceTests()
        {
            _service = new AlunoService(_usuarioRepositoryMock.Object, _areaInteresseRepositoryMock.Object);
        }

        private static Usuario NovoUsuario(int id = 10) => new()
        {
            Id = id,
            Nome = "Aluno Teste",
            Email = "aluno.teste@knowhow.com",
            DataNascimento = new DateTime(2003, 5, 14),
            AreaInteresseId = 1,
            AreaInteresse = new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" }
        };

        private static AlunoPerfilRequestDTO PerfilRequestValido() => new()
        {
            Nome = "Aluno Editado",
            Email = "aluno.editado@knowhow.com",
            DataNascimento = new DateTime(2003, 5, 14),
            AreaInteresse = "Design"
        };

        [Fact]
        public async Task ObterPerfil_UsuarioExistente_RetornaDadosDoUsuario()
        {
            var usuario = NovoUsuario();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);

            var resultado = await _service.ObterPerfil(10);

            Assert.NotNull(resultado);
            Assert.Equal(10, resultado!.Id);
            Assert.Equal("Aluno Teste", resultado.Nome);
            Assert.Equal("aluno.teste@knowhow.com", resultado.Email);
            Assert.Equal("Tecnologia e Programação", resultado.AreaInteresse);
            Assert.Null(resultado.FotoPerfilBase64);
        }

        [Fact]
        public async Task ObterPerfil_UsuarioInexistente_RetornaNull()
        {
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(99)).ReturnsAsync((Usuario?)null);

            var resultado = await _service.ObterPerfil(99);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task AtualizarPerfil_DadosValidos_AtualizaUsuarioERetornaDadosAtualizados()
        {
            var usuario = NovoUsuario();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail("aluno.editado@knowhow.com")).ReturnsAsync((Usuario?)null);
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Design"))
                .ReturnsAsync(new AreaInteresse { Id = 2, Nome = "Design" });
            _usuarioRepositoryMock.Setup(r => r.Update(It.IsAny<Usuario>())).ReturnsAsync((Usuario u) => u);

            var resultado = await _service.AtualizarPerfil(10, PerfilRequestValido());

            Assert.NotNull(resultado);
            Assert.Equal("Aluno Editado", resultado!.Nome);
            Assert.Equal("aluno.editado@knowhow.com", resultado.Email);
            Assert.Equal("Design", resultado.AreaInteresse);
            _usuarioRepositoryMock.Verify(r => r.Update(It.Is<Usuario>(u =>
                u.Nome == "Aluno Editado" && u.AreaInteresseId == 2)), Times.Once);
        }

        [Fact]
        public async Task AtualizarPerfil_UsuarioInexistente_RetornaNull()
        {
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(99)).ReturnsAsync((Usuario?)null);

            var resultado = await _service.AtualizarPerfil(99, PerfilRequestValido());

            Assert.Null(resultado);
        }

        [Fact]
        public async Task AtualizarPerfil_EmailJaUsadoPorOutroUsuario_LancaDomainValidationException()
        {
            var usuario = NovoUsuario();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail("aluno.editado@knowhow.com"))
                .ReturnsAsync(new Usuario { Id = 20, Email = "aluno.editado@knowhow.com" });

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPerfil(10, PerfilRequestValido()));
            _usuarioRepositoryMock.Verify(r => r.Update(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarPerfil_AreaInteresseInexistente_LancaDomainValidationException()
        {
            var usuario = NovoUsuario();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail("aluno.editado@knowhow.com")).ReturnsAsync((Usuario?)null);
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Área que não existe")).ReturnsAsync((AreaInteresse?)null);
            var request = PerfilRequestValido();
            request.AreaInteresse = "Área que não existe";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPerfil(10, request));
            _usuarioRepositoryMock.Verify(r => r.Update(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarPerfil_NomeVazio_LancaDomainValidationException()
        {
            var usuario = NovoUsuario();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            var request = PerfilRequestValido();
            request.Nome = "   ";

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPerfil(10, request));
            _usuarioRepositoryMock.Verify(r => r.Update(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarPerfil_DataNascimentoFutura_LancaDomainValidationException()
        {
            var usuario = NovoUsuario();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            var request = PerfilRequestValido();
            request.DataNascimento = DateTime.UtcNow.AddDays(1);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.AtualizarPerfil(10, request));
            _usuarioRepositoryMock.Verify(r => r.Update(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarPerfil_ComFotoBase64_AtualizaFotoEContentType()
        {
            var usuario = NovoUsuario();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail("aluno.editado@knowhow.com")).ReturnsAsync((Usuario?)null);
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Design"))
                .ReturnsAsync(new AreaInteresse { Id = 2, Nome = "Design" });
            _usuarioRepositoryMock.Setup(r => r.Update(It.IsAny<Usuario>())).ReturnsAsync((Usuario u) => u);
            var request = PerfilRequestValido();
            request.FotoPerfilBase64 = "data:image/png;base64,iVBORw0KGgo=";

            var resultado = await _service.AtualizarPerfil(10, request);

            Assert.NotNull(resultado);
            Assert.StartsWith("data:image/png;base64,", resultado!.FotoPerfilBase64);
            _usuarioRepositoryMock.Verify(r => r.Update(It.Is<Usuario>(u =>
                u.FotoPerfilContentType == "image/png" && u.FotoPerfil != null)), Times.Once);
        }

        [Fact]
        public async Task AtualizarPerfil_SemFotoBase64_MantemFotoExistente()
        {
            var usuario = NovoUsuario();
            usuario.FotoPerfil = new byte[] { 1, 2, 3 };
            usuario.FotoPerfilContentType = "image/jpeg";
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(10)).ReturnsAsync(usuario);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail("aluno.editado@knowhow.com")).ReturnsAsync((Usuario?)null);
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Design"))
                .ReturnsAsync(new AreaInteresse { Id = 2, Nome = "Design" });
            _usuarioRepositoryMock.Setup(r => r.Update(It.IsAny<Usuario>())).ReturnsAsync((Usuario u) => u);

            var resultado = await _service.AtualizarPerfil(10, PerfilRequestValido());

            Assert.NotNull(resultado);
            Assert.Equal("data:image/jpeg;base64,AQID", resultado!.FotoPerfilBase64);
        }
    }
}
