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
        private readonly Mock<IAreaInteresseRepository> _areaInteresseRepositoryMock = new();
        private readonly Mock<IProfessorRepository> _professorRepositoryMock = new();
        private readonly Mock<ICryptography> _cryptographyMock = new();
        private readonly UsuarioService _service;

        public UsuarioServiceTests()
        {
            _cryptographyMock.Setup(c => c.Crypt(It.IsAny<string>())).Returns("hash-fake");
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome("Tecnologia e Programação"))
                .ReturnsAsync(new AreaInteresse { Id = 1, Nome = "Tecnologia e Programação" });
            _professorRepositoryMock.Setup(r => r.CriarPerfilProfessor(It.IsAny<PerfilProfessor>()))
                .ReturnsAsync((PerfilProfessor p) => p);

            var jwtSettings = new JWTSettings
            {
                Issuer = "KnowHowApi",
                Audience = "KnowHowApiClient",
                SecretKey = "chave-secreta-de-teste-com-tamanho-suficiente"
            };

            _service = new UsuarioService(_usuarioRepositoryMock.Object, _areaInteresseRepositoryMock.Object, _professorRepositoryMock.Object, _cryptographyMock.Object, jwtSettings);
        }

        private static RegisterUsuarioDTO NovoAlunoValido() => new()
        {
            Nome = "Aluno Teste",
            Email = "aluno.teste@knowhow.com",
            Senha = "Senha@123",
            TipoUsuario = TipoUsuario.Aluno,
            DataNascimento = new DateTime(2003, 5, 14),
            AreaInteresse = "Tecnologia e Programação"
        };

        private static RegisterUsuarioDTO NovoProfessorValido() => new()
        {
            Nome = "Professor Teste",
            Email = "professor.teste@knowhow.com",
            Senha = "Senha@123",
            TipoUsuario = TipoUsuario.Professor,
            DataNascimento = new DateTime(1988, 11, 2),
            Cpf = "71428793860",
            AreaEspecialidade = "Tecnologia e Programação",
            AnosExperiencia = "3 a 5 anos",
            MiniBiografia = "Professor com experiência em desenvolvimento de software e ensino."
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
        public async Task Registrar_AlunoSemAreaInteresse_LancaBadRequest()
        {
            var request = NovoAlunoValido();
            request.AreaInteresse = null;

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Registrar_AlunoComAreaInteresseInvalida_LancaBadRequest()
        {
            var request = NovoAlunoValido();
            request.AreaInteresse = "Área Inexistente";
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome(request.AreaInteresse))
                .ReturnsAsync((AreaInteresse?)null);

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
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
            _professorRepositoryMock.Verify(r => r.CriarPerfilProfessor(It.Is<PerfilProfessor>(p =>
                p.Materia == request.AreaEspecialidade &&
                p.AreaEspecialidadeId == 1 &&
                p.AnosExperiencia == request.AnosExperiencia &&
                p.Descricao == request.MiniBiografia)), Times.Once);
        }

        [Fact]
        public async Task Registrar_ProfessorSemAreaEspecialidade_LancaBadRequest()
        {
            var request = NovoProfessorValido();
            request.AreaEspecialidade = null;
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf(request.Cpf!)).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Registrar_ProfessorComAreaEspecialidadeInvalida_LancaBadRequest()
        {
            var request = NovoProfessorValido();
            request.AreaEspecialidade = "Área Inexistente";
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf(request.Cpf!)).ReturnsAsync((Usuario?)null);
            _areaInteresseRepositoryMock.Setup(r => r.GetAreaInteresseByNome(request.AreaEspecialidade))
                .ReturnsAsync((AreaInteresse?)null);

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Registrar_ProfessorSemAnosExperiencia_LancaBadRequest()
        {
            var request = NovoProfessorValido();
            request.AnosExperiencia = null;
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf(request.Cpf!)).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Registrar_ProfessorSemMiniBiografia_LancaBadRequest()
        {
            var request = NovoProfessorValido();
            request.MiniBiografia = null;
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf(request.Cpf!)).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task Registrar_ProfessorComFotoPerfilDataUrlValida_PersisteBytesEContentType()
        {
            var request = NovoProfessorValido();
            request.FotoPerfilBase64 = "data:image/png;base64,iVBORw0KGgo=";
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByEmail(request.Email)).ReturnsAsync((Usuario?)null);
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf(request.Cpf!)).ReturnsAsync((Usuario?)null);
            _usuarioRepositoryMock.Setup(r => r.CriarUsuario(It.IsAny<Usuario>())).ReturnsAsync((Usuario u) => u);

            await _service.Registrar(request);

            _professorRepositoryMock.Verify(r => r.CriarPerfilProfessor(It.Is<PerfilProfessor>(p =>
                p.FotoPerfilContentType == "image/png" &&
                p.FotoPerfil != null && p.FotoPerfil.Length > 0)), Times.Once);
        }

        [Fact]
        public async Task Registrar_ProfessorComFotoPerfilInvalida_LancaBadRequest()
        {
            var request = NovoProfessorValido();
            request.FotoPerfilBase64 = "data:image/png;base64,isso-nao-e-base64!!!";
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioByCpf(request.Cpf!)).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<BadHttpRequestException>(() => _service.Registrar(request));
            _usuarioRepositoryMock.Verify(r => r.CriarUsuario(It.IsAny<Usuario>()), Times.Never);
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
