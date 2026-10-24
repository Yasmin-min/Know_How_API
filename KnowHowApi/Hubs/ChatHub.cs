using System.Security.Claims;
using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KnowHowApi.Hubs
{
    // Cada conexão autenticada entra automaticamente no grupo do próprio usuário.
    // O ChatService publica os eventos de novas mensagens/leitura nesses grupos,
    // então o cliente só precisa conectar e ouvir — sem gerenciar grupos por conversa.
    //
    // O mesmo hub faz a sinalização das chamadas de vídeo (WebRTC): o vídeo vai direto
    // entre os navegadores; por aqui passam só os sinais (offer/answer/ICE) entre os dois
    // participantes da aula.
    [Authorize]
    public class ChatHub : Hub
    {
        // O SignalR recusa mensagens acima de 32 KB; um SDP fica bem abaixo disso.
        private const int SinalMaxLength = 30_000;

        private readonly IChamadaService _chamadaService;
        private readonly SalasChamada _salas;

        public ChatHub(IChamadaService chamadaService, SalasChamada salas)
        {
            _chamadaService = chamadaService;
            _salas = salas;
        }

        public override async Task OnConnectedAsync()
        {
            var usuarioId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(usuarioId))
                await Groups.AddToGroupAsync(Context.ConnectionId, GrupoUsuario(usuarioId));

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var (aulaId, usuarioId) in _salas.RemoverConexao(Context.ConnectionId))
                await Clients.Group(GrupoChamada(aulaId)).SendAsync("ChamadaParticipanteSaiu", aulaId.ToString(), usuarioId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task<EntradaChamadaDTO> EntrarNaChamada(string aulaId)
        {
            var usuarioId = UsuarioId();
            var id = LerAulaId(aulaId);

            try
            {
                await _chamadaService.ValidarEntrada(usuarioId, id);
            }
            catch (Exception ex) when (ex is ForbiddenException or NotFoundException)
            {
                throw new HubException(ex.Message);
            }

            var grupo = GrupoChamada(id);
            var resultado = _salas.Entrar(id, Context.ConnectionId, usuarioId);

            // A conexão antiga do mesmo usuário (outra aba/aparelho) sai da sala antes do aviso.
            foreach (var conexaoAntiga in resultado.ConexoesSubstituidas)
            {
                await Groups.RemoveFromGroupAsync(conexaoAntiga, grupo);
                await Clients.Client(conexaoAntiga).SendAsync("ChamadaAbertaEmOutroDispositivo", aulaId);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
            await Clients.OthersInGroup(grupo).SendAsync("ChamadaParticipanteEntrou", aulaId, usuarioId);

            return new EntradaChamadaDTO { OutroPresente = resultado.OutroPresente };
        }

        public async Task SairDaChamada(string aulaId)
        {
            var id = LerAulaId(aulaId);
            if (!_salas.Sair(id, Context.ConnectionId))
                return;

            var grupo = GrupoChamada(id);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);
            await Clients.Group(grupo).SendAsync("ChamadaParticipanteSaiu", aulaId, UsuarioId());
        }

        // Repassa um sinal WebRTC (JSON opaco) para o outro participante da sala.
        public async Task EnviarSinalChamada(string aulaId, string sinal)
        {
            var id = LerAulaId(aulaId);
            if (!_salas.Contem(id, Context.ConnectionId))
                throw new HubException("Você não está nesta chamada.");

            if (string.IsNullOrEmpty(sinal) || sinal.Length > SinalMaxLength)
                throw new HubException("Sinal inválido.");

            await Clients.OthersInGroup(GrupoChamada(id)).SendAsync("ChamadaSinal", aulaId, UsuarioId(), sinal);
        }

        private int UsuarioId()
        {
            return int.TryParse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? id
                : throw new HubException("Usuário não autenticado.");
        }

        private static Guid LerAulaId(string aulaId)
        {
            return Guid.TryParse(aulaId, out var id) ? id : throw new HubException("Aula inválida.");
        }

        public static string GrupoUsuario(int usuarioId) => GrupoUsuario(usuarioId.ToString());

        public static string GrupoUsuario(string usuarioId) => $"usuario-{usuarioId}";

        public static string GrupoChamada(Guid aulaId) => $"chamada-{aulaId}";
    }
}
