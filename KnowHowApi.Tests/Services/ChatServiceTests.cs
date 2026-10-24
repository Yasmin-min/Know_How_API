using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Hubs;
using KnowHowApi.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IChatRepository> _chatRepositoryMock = new();
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
        private readonly Mock<IProfessorRepository> _professorRepositoryMock = new();
        private readonly Mock<IHubContext<ChatHub>> _hubContextMock = new();
        private readonly Mock<IClientProxy> _clientProxyMock = new();
        private readonly Mock<IProfessorAlunoRepository> _professorAlunoRepositoryMock = new();
        private readonly ChatService _service;

        public ChatServiceTests()
        {
            var clientsMock = new Mock<IHubClients>();
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
            _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            _clientProxyMock
                .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _service = new ChatService(_chatRepositoryMock.Object, _usuarioRepositoryMock.Object, _professorRepositoryMock.Object, _hubContextMock.Object, _professorAlunoRepositoryMock.Object);
        }

        private static Conversa NovaConversa(int id = 1, int alunoId = 10, int professorId = 20) => new()
        {
            Id = id,
            AlunoId = alunoId,
            Aluno = new Usuario { Id = alunoId, Nome = "Aluno Teste", Email = "aluno@knowhow.com" },
            ProfessorId = professorId,
            Professor = new Usuario { Id = professorId, Nome = "Professor Teste", Email = "professor@knowhow.com" },
            CriadaEm = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        [Fact]
        public async Task ListarConversas_UsuarioEhAluno_RetornaProfessorComoParticipante()
        {
            var conversa = NovaConversa();
            _chatRepositoryMock.Setup(r => r.ListarConversasDoUsuario(10)).ReturnsAsync(new List<Conversa> { conversa });
            _chatRepositoryMock.Setup(r => r.ObterUltimasMensagens(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new Dictionary<int, Mensagem>
                {
                    [1] = new Mensagem { Id = 5, ConversaId = 1, RemetenteId = 20, Texto = "Oi", EnviadaEm = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) }
                });
            _chatRepositoryMock.Setup(r => r.ContarNaoLidasPorConversa(It.IsAny<IEnumerable<int>>(), 10))
                .ReturnsAsync(new Dictionary<int, int> { [1] = 3 });
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(20)).ReturnsAsync((PerfilProfessor?)null);

            var resultado = await _service.ListarConversas(10);

            Assert.Single(resultado);
            var resumo = resultado[0];
            Assert.Equal("1", resumo.Id);
            Assert.Equal(20, resumo.ParticipanteId);
            Assert.Equal("Professor Teste", resumo.ParticipanteNome);
            Assert.Equal("Oi", resumo.UltimaMensagem);
            Assert.Equal(3, resumo.NaoLidas);
        }

        [Fact]
        public async Task ListarConversas_UsuarioEhProfessor_RetornaAlunoComoParticipante()
        {
            var conversa = NovaConversa();
            _chatRepositoryMock.Setup(r => r.ListarConversasDoUsuario(20)).ReturnsAsync(new List<Conversa> { conversa });
            _chatRepositoryMock.Setup(r => r.ObterUltimasMensagens(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new Dictionary<int, Mensagem>());
            _chatRepositoryMock.Setup(r => r.ContarNaoLidasPorConversa(It.IsAny<IEnumerable<int>>(), 20))
                .ReturnsAsync(new Dictionary<int, int>());

            var resultado = await _service.ListarConversas(20);

            Assert.Single(resultado);
            var resumo = resultado[0];
            Assert.Equal(10, resumo.ParticipanteId);
            Assert.Equal("Aluno Teste", resumo.ParticipanteNome);
            Assert.Equal(string.Empty, resumo.UltimaMensagem);
            Assert.Equal(0, resumo.NaoLidas);
            _professorRepositoryMock.Verify(r => r.GetPerfilProfessorByUsuarioId(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ListarConversas_SemConversas_RetornaListaVazia()
        {
            _chatRepositoryMock.Setup(r => r.ListarConversasDoUsuario(99)).ReturnsAsync(new List<Conversa>());

            var resultado = await _service.ListarConversas(99);

            Assert.Empty(resultado);
        }

        [Fact]
        public async Task ListarMensagens_UsuarioNaoParticipante_RetornaNull()
        {
            _chatRepositoryMock.Setup(r => r.GetConversaById(1)).ReturnsAsync(NovaConversa());

            var resultado = await _service.ListarMensagens(1, 999);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ListarMensagens_ConversaInexistente_RetornaNull()
        {
            _chatRepositoryMock.Setup(r => r.GetConversaById(1)).ReturnsAsync((Conversa?)null);

            var resultado = await _service.ListarMensagens(1, 10);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task ListarMensagens_Participante_RetornaMensagensMapeadas()
        {
            _chatRepositoryMock.Setup(r => r.GetConversaById(1)).ReturnsAsync(NovaConversa());
            _chatRepositoryMock.Setup(r => r.ListarMensagens(1)).ReturnsAsync(new List<Mensagem>
            {
                new Mensagem { Id = 1, ConversaId = 1, RemetenteId = 10, Texto = "Olá", EnviadaEm = DateTime.UtcNow, Lida = true }
            });

            var resultado = await _service.ListarMensagens(1, 10);

            Assert.NotNull(resultado);
            Assert.Single(resultado!);
            Assert.Equal("1", resultado![0].Id);
            Assert.Equal("Olá", resultado[0].Texto);
            Assert.True(resultado[0].Lida);
        }

        [Fact]
        public async Task EnviarMensagem_UsuarioNaoParticipante_RetornaNull()
        {
            _chatRepositoryMock.Setup(r => r.GetConversaById(1)).ReturnsAsync(NovaConversa());

            var resultado = await _service.EnviarMensagem(1, 999, new EnviarMensagemRequestDTO { Texto = "Oi" });

            Assert.Null(resultado);
            _chatRepositoryMock.Verify(r => r.CriarMensagem(It.IsAny<Mensagem>()), Times.Never);
        }

        [Fact]
        public async Task EnviarMensagem_TextoVazio_LancaDomainValidationException()
        {
            _chatRepositoryMock.Setup(r => r.GetConversaById(1)).ReturnsAsync(NovaConversa());

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.EnviarMensagem(1, 10, new EnviarMensagemRequestDTO { Texto = "   " }));
            _chatRepositoryMock.Verify(r => r.CriarMensagem(It.IsAny<Mensagem>()), Times.Never);
        }

        [Fact]
        public async Task EnviarMensagem_TextoMuitoLongo_LancaDomainValidationException()
        {
            _chatRepositoryMock.Setup(r => r.GetConversaById(1)).ReturnsAsync(NovaConversa());

            var request = new EnviarMensagemRequestDTO { Texto = new string('a', 2001) };

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.EnviarMensagem(1, 10, request));
            _chatRepositoryMock.Verify(r => r.CriarMensagem(It.IsAny<Mensagem>()), Times.Never);
        }

        [Fact]
        public async Task EnviarMensagem_Valido_CriaMensagemENotificaAmbosParticipantes()
        {
            _chatRepositoryMock.Setup(r => r.GetConversaById(1)).ReturnsAsync(NovaConversa());
            _chatRepositoryMock.Setup(r => r.CriarMensagem(It.IsAny<Mensagem>()))
                .ReturnsAsync((Mensagem m) => { m.Id = 7; return m; });

            var resultado = await _service.EnviarMensagem(1, 10, new EnviarMensagemRequestDTO { Texto = "  Olá, professor!  " });

            Assert.NotNull(resultado);
            Assert.Equal("7", resultado!.Id);
            Assert.Equal("Olá, professor!", resultado.Texto);
            Assert.Equal(10, resultado.RemetenteId);

            _chatRepositoryMock.Verify(r => r.CriarMensagem(It.Is<Mensagem>(m =>
                m.ConversaId == 1 && m.RemetenteId == 10 && m.Texto == "Olá, professor!")), Times.Once);

            _clientProxyMock.Verify(p => p.SendCoreAsync("NovaMensagem", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task IniciarConversa_ProfessorInexistente_LancaDomainValidationException()
        {
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(20)).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.IniciarConversa(10, new IniciarConversaRequestDTO { ProfessorId = 20 }));
            _chatRepositoryMock.Verify(r => r.CriarConversa(It.IsAny<Conversa>()), Times.Never);
        }

        [Fact]
        public async Task IniciarConversa_UsuarioNaoEhProfessor_LancaDomainValidationException()
        {
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(20))
                .ReturnsAsync(new Usuario { Id = 20, Nome = "Outro Aluno", TipoUsuario = TipoUsuario.Aluno });

            await Assert.ThrowsAsync<DomainValidationException>(() => _service.IniciarConversa(10, new IniciarConversaRequestDTO { ProfessorId = 20 }));
            _chatRepositoryMock.Verify(r => r.CriarConversa(It.IsAny<Conversa>()), Times.Never);
        }

        [Fact]
        public async Task IniciarConversa_ConversaNovaComProfessorValido_CriaERetornaResumo()
        {
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(20))
                .ReturnsAsync(new Usuario { Id = 20, Nome = "Professor Teste", TipoUsuario = TipoUsuario.Professor });
            _chatRepositoryMock.Setup(r => r.GetConversaEntreAlunoEProfessor(10, 20)).ReturnsAsync((Conversa?)null);
            _chatRepositoryMock.Setup(r => r.CriarConversa(It.IsAny<Conversa>()))
                .ReturnsAsync((Conversa c) => { c.Id = 5; return c; });
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(20)).ReturnsAsync((PerfilProfessor?)null);
            _chatRepositoryMock.Setup(r => r.ObterUltimasMensagens(It.IsAny<IEnumerable<int>>())).ReturnsAsync(new Dictionary<int, Mensagem>());
            _chatRepositoryMock.Setup(r => r.ContarNaoLidasPorConversa(It.IsAny<IEnumerable<int>>(), 10)).ReturnsAsync(new Dictionary<int, int>());

            var resultado = await _service.IniciarConversa(10, new IniciarConversaRequestDTO { ProfessorId = 20 });

            Assert.Equal("5", resultado.Id);
            Assert.Equal(20, resultado.ParticipanteId);
            Assert.Equal("Professor Teste", resultado.ParticipanteNome);
            _chatRepositoryMock.Verify(r => r.CriarConversa(It.Is<Conversa>(c => c.AlunoId == 10 && c.ProfessorId == 20)), Times.Once);
        }

        [Fact]
        public async Task IniciarConversa_ConversaJaExiste_RetornaExistenteSemCriarNova()
        {
            var existente = NovaConversa();
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(20))
                .ReturnsAsync(new Usuario { Id = 20, Nome = "Professor Teste", TipoUsuario = TipoUsuario.Professor });
            _chatRepositoryMock.Setup(r => r.GetConversaEntreAlunoEProfessor(10, 20)).ReturnsAsync(existente);
            _professorRepositoryMock.Setup(r => r.GetPerfilProfessorByUsuarioId(20)).ReturnsAsync((PerfilProfessor?)null);
            _chatRepositoryMock.Setup(r => r.ObterUltimasMensagens(It.IsAny<IEnumerable<int>>())).ReturnsAsync(new Dictionary<int, Mensagem>());
            _chatRepositoryMock.Setup(r => r.ContarNaoLidasPorConversa(It.IsAny<IEnumerable<int>>(), 10)).ReturnsAsync(new Dictionary<int, int>());

            var resultado = await _service.IniciarConversa(10, new IniciarConversaRequestDTO { ProfessorId = 20 });

            Assert.Equal(existente.Id.ToString(), resultado.Id);
            _chatRepositoryMock.Verify(r => r.CriarConversa(It.IsAny<Conversa>()), Times.Never);
        }

        private void AlunoNaListaDoProfessor(int professorId = 20, int alunoId = 10)
        {
            _professorAlunoRepositoryMock.Setup(r => r.GetAlunoDoProfessor(professorId, alunoId))
                .ReturnsAsync(new Usuario { Id = alunoId, Nome = "Aluno Teste" });
            _usuarioRepositoryMock.Setup(r => r.GetUsuarioById(alunoId))
                .ReturnsAsync(new Usuario { Id = alunoId, Nome = "Aluno Teste", TipoUsuario = TipoUsuario.Aluno, FotoPerfil = new byte[] { 1, 2 }, FotoPerfilContentType = "image/png" });
            _chatRepositoryMock.Setup(r => r.ObterUltimasMensagens(It.IsAny<IEnumerable<int>>())).ReturnsAsync(new Dictionary<int, Mensagem>());
        }

        [Fact]
        public async Task IniciarConversaComAluno_ConversaNova_CriaERetornaAlunoComoParticipante()
        {
            AlunoNaListaDoProfessor();
            _chatRepositoryMock.Setup(r => r.GetConversaEntreAlunoEProfessor(10, 20)).ReturnsAsync((Conversa?)null);
            _chatRepositoryMock.Setup(r => r.CriarConversa(It.IsAny<Conversa>()))
                .ReturnsAsync((Conversa c) => { c.Id = 7; return c; });
            _chatRepositoryMock.Setup(r => r.ContarNaoLidasPorConversa(It.IsAny<IEnumerable<int>>(), 20))
                .ReturnsAsync(new Dictionary<int, int> { [7] = 2 });

            var resultado = await _service.IniciarConversaComAluno(20, new IniciarConversaComAlunoRequestDTO { AlunoId = 10 });

            Assert.Equal("7", resultado.Id);
            Assert.Equal(10, resultado.ParticipanteId);
            Assert.Equal("Aluno Teste", resultado.ParticipanteNome);
            Assert.StartsWith("data:image/png;base64,", resultado.ParticipanteFotoUrl);
            Assert.Equal(2, resultado.NaoLidas);
            _chatRepositoryMock.Verify(r => r.CriarConversa(It.Is<Conversa>(c => c.AlunoId == 10 && c.ProfessorId == 20)), Times.Once);
            _chatRepositoryMock.Verify(r => r.ContarNaoLidasPorConversa(It.IsAny<IEnumerable<int>>(), 20), Times.Once);
        }

        [Fact]
        public async Task IniciarConversaComAluno_ConversaJaExiste_RetornaExistenteSemCriarNova()
        {
            AlunoNaListaDoProfessor();
            var existente = NovaConversa();
            _chatRepositoryMock.Setup(r => r.GetConversaEntreAlunoEProfessor(10, 20)).ReturnsAsync(existente);
            _chatRepositoryMock.Setup(r => r.ContarNaoLidasPorConversa(It.IsAny<IEnumerable<int>>(), 20)).ReturnsAsync(new Dictionary<int, int>());

            var resultado = await _service.IniciarConversaComAluno(20, new IniciarConversaComAlunoRequestDTO { AlunoId = 10 });

            Assert.Equal(existente.Id.ToString(), resultado.Id);
            _chatRepositoryMock.Verify(r => r.CriarConversa(It.IsAny<Conversa>()), Times.Never);
        }

        [Fact]
        public async Task IniciarConversaComAluno_AlunoForaDaLista_LancaValidacao()
        {
            _professorAlunoRepositoryMock.Setup(r => r.GetAlunoDoProfessor(20, 10)).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.IniciarConversaComAluno(20, new IniciarConversaComAlunoRequestDTO { AlunoId = 10 }));
            _chatRepositoryMock.Verify(r => r.CriarConversa(It.IsAny<Conversa>()), Times.Never);
        }

        [Fact]
        public async Task IniciarConversaComAluno_AlunoIdInvalido_LancaValidacao()
        {
            await Assert.ThrowsAsync<DomainValidationException>(() =>
                _service.IniciarConversaComAluno(20, new IniciarConversaComAlunoRequestDTO { AlunoId = 0 }));
        }

        [Fact]
        public async Task MarcarComoLida_Participante_RetornaTrueENotificaAmbos()
        {
            _chatRepositoryMock.Setup(r => r.GetConversaById(1)).ReturnsAsync(NovaConversa());
            _chatRepositoryMock.Setup(r => r.MarcarMensagensComoLidas(1, 10)).ReturnsAsync(2);

            var resultado = await _service.MarcarComoLida(1, 10);

            Assert.True(resultado);
            _clientProxyMock.Verify(p => p.SendCoreAsync("MensagensLidas", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task MarcarComoLida_SemMensagensParaAtualizar_NaoNotifica()
        {
            _chatRepositoryMock.Setup(r => r.GetConversaById(1)).ReturnsAsync(NovaConversa());
            _chatRepositoryMock.Setup(r => r.MarcarMensagensComoLidas(1, 10)).ReturnsAsync(0);

            var resultado = await _service.MarcarComoLida(1, 10);

            Assert.True(resultado);
            _clientProxyMock.Verify(p => p.SendCoreAsync("MensagensLidas", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task MarcarComoLida_UsuarioNaoParticipante_RetornaNull()
        {
            _chatRepositoryMock.Setup(r => r.GetConversaById(1)).ReturnsAsync(NovaConversa());

            var resultado = await _service.MarcarComoLida(1, 999);

            Assert.Null(resultado);
            _chatRepositoryMock.Verify(r => r.MarcarMensagensComoLidas(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
