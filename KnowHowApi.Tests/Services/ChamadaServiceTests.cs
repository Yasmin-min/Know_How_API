using System.Text.Json;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Hubs;
using KnowHowApi.Services;
using KnowHowApi.Services.Interfaces;
using KnowHowApi.Services.Utils;
using Moq;

namespace KnowHowApi.Tests.Services
{
    public class ChamadaServiceTests
    {
        private const int ProfessorId = 10;
        private const int AlunoId = 12;

        private readonly Mock<IAulaAgendadaRepository> _repositoryMock = new();
        private readonly Mock<ICredenciaisTurnService> _turnMock = new();
        private readonly ChamadaService _service;
        private readonly Guid _aulaId = Guid.NewGuid();

        public ChamadaServiceTests()
        {
            _service = new ChamadaService(_repositoryMock.Object, _turnMock.Object);
            _turnMock.Setup(t => t.GerarIceServers()).ReturnsAsync(new List<IceServerDTO>
            {
                new() { Urls = new() { "turn:turn.exemplo:3478" }, Username = "u", Credential = "c" }
            });
        }

        private void AulaComecandoEm(TimeSpan deslocamento, string status = AulaAgendada.StatusAgendada)
        {
            var inicio = DateTime.UtcNow.Add(deslocamento);
            _repositoryMock.Setup(r => r.GetByIdComParticipantes(_aulaId)).ReturnsAsync(new AulaAgendada
            {
                Id = _aulaId,
                ProfessorId = ProfessorId,
                Professor = new Usuario { Id = ProfessorId, Nome = "Marina Costa" },
                AlunoId = AlunoId,
                Aluno = new Usuario { Id = AlunoId, Nome = "Diego Rocha" },
                Titulo = "Inglês",
                Inicio = inicio,
                Fim = inicio.AddMinutes(60),
                DuracaoMinutos = 60,
                Status = status
            });
        }

        [Fact]
        public async Task ObterAcesso_SalaAberta_DevolveOutroParticipanteEIceServers()
        {
            AulaComecandoEm(TimeSpan.FromMinutes(5));

            var acesso = await _service.ObterAcesso(AlunoId, _aulaId);

            Assert.Equal(ChamadaAcessoDTO.SituacaoAberta, acesso.Situacao);
            Assert.False(acesso.SouProfessor);
            Assert.Equal(ProfessorId, acesso.Participante.Id);
            Assert.Equal("Marina Costa", acesso.Participante.Nome);
            Assert.Single(acesso.IceServers);
        }

        [Fact]
        public async Task ObterAcesso_ProfessorVeOAluno()
        {
            AulaComecandoEm(TimeSpan.Zero);

            var acesso = await _service.ObterAcesso(ProfessorId, _aulaId);

            Assert.True(acesso.SouProfessor);
            Assert.Equal("Diego Rocha", acesso.Participante.Nome);
        }

        [Fact]
        public async Task ObterAcesso_AntesDeAbrir_AguardandoSemCredenciais()
        {
            AulaComecandoEm(TimeSpan.FromHours(2));

            var acesso = await _service.ObterAcesso(AlunoId, _aulaId);

            Assert.Equal(ChamadaAcessoDTO.SituacaoAguardando, acesso.Situacao);
            Assert.Empty(acesso.IceServers);
            _turnMock.Verify(t => t.GerarIceServers(), Times.Never);
        }

        [Fact]
        public async Task ObterAcesso_DepoisDaTolerancia_Encerrada()
        {
            AulaComecandoEm(TimeSpan.FromMinutes(-(60 + ChamadaService.MinutosDepoisDoFim + 1)));

            var acesso = await _service.ObterAcesso(AlunoId, _aulaId);

            Assert.Equal(ChamadaAcessoDTO.SituacaoEncerrada, acesso.Situacao);
        }

        [Fact]
        public async Task ObterAcesso_UsuarioDeFora_LancaNotFound()
        {
            AulaComecandoEm(TimeSpan.Zero);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterAcesso(99, _aulaId));
        }

        [Fact]
        public async Task ValidarEntrada_Cancelada_LancaForbidden()
        {
            AulaComecandoEm(TimeSpan.Zero, AulaAgendada.StatusCancelada);

            var erro = await Assert.ThrowsAsync<ForbiddenException>(() => _service.ValidarEntrada(AlunoId, _aulaId));
            Assert.Contains("cancelada", erro.Message);
        }

        [Fact]
        public async Task ValidarEntrada_AntesDeAbrir_LancaForbidden()
        {
            AulaComecandoEm(TimeSpan.FromMinutes(ChamadaService.MinutosAntesDeAbrir + 5));

            await Assert.ThrowsAsync<ForbiddenException>(() => _service.ValidarEntrada(ProfessorId, _aulaId));
        }

        [Fact]
        public async Task ValidarEntrada_DentroDaJanela_NaoLanca()
        {
            AulaComecandoEm(TimeSpan.FromMinutes(ChamadaService.MinutosAntesDeAbrir - 1));

            await _service.ValidarEntrada(ProfessorId, _aulaId);
        }
    }

    public class SalasChamadaTests
    {
        private readonly SalasChamada _salas = new();
        private readonly Guid _aula = Guid.NewGuid();

        [Fact]
        public void Entrar_PrimeiroSozinho_SegundoEncontraOutro()
        {
            Assert.False(_salas.Entrar(_aula, "c1", 10).OutroPresente);
            Assert.True(_salas.Entrar(_aula, "c2", 12).OutroPresente);
        }

        [Fact]
        public void Entrar_MesmoUsuarioDeOutraConexao_SubstituiAAnterior()
        {
            _salas.Entrar(_aula, "c1", 10);

            var resultado = _salas.Entrar(_aula, "c3", 10);

            Assert.Equal(new[] { "c1" }, resultado.ConexoesSubstituidas);
            Assert.False(resultado.OutroPresente);
            Assert.False(_salas.Contem(_aula, "c1"));
            Assert.True(_salas.Contem(_aula, "c3"));
        }

        [Fact]
        public void RemoverConexao_DevolveSalasDeQueSaiu()
        {
            _salas.Entrar(_aula, "c1", 10);
            _salas.Entrar(_aula, "c2", 12);

            var saidas = _salas.RemoverConexao("c2");

            Assert.Equal(new[] { (_aula, 12) }, saidas);
            Assert.False(_salas.Contem(_aula, "c2"));
            Assert.False(_salas.Sair(_aula, "c2"));
        }
    }

    public class CloudflareTurnServiceTests
    {
        [Fact]
        public void LerIceServers_ListaRemovePorta53()
        {
            var json = JsonDocument.Parse("""
                {"iceServers":[
                  {"urls":["stun:stun.cloudflare.com:3478","stun:stun.cloudflare.com:53"]},
                  {"urls":["turn:turn.cloudflare.com:3478?transport=udp","turn:turn.cloudflare.com:53?transport=udp","turns:turn.cloudflare.com:443?transport=tcp"],
                   "username":"usuario","credential":"senha"}
                ]}
                """);

            var servidores = CloudflareTurnService.LerIceServers(json.RootElement);

            Assert.Equal(2, servidores.Count);
            Assert.Equal(new[] { "stun:stun.cloudflare.com:3478" }, servidores[0].Urls);
            Assert.DoesNotContain(servidores[1].Urls, u => u.Contains(":53"));
            Assert.Equal("usuario", servidores[1].Username);
            Assert.Equal("senha", servidores[1].Credential);
        }

        [Fact]
        public void LerIceServers_ObjetoUnico()
        {
            var json = JsonDocument.Parse("""{"iceServers":{"urls":"turn:turn.cloudflare.com:3478","username":"u","credential":"c"}}""");

            var servidor = Assert.Single(CloudflareTurnService.LerIceServers(json.RootElement));

            Assert.Equal(new[] { "turn:turn.cloudflare.com:3478" }, servidor.Urls);
        }
    }
}
