using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Hubs;
using KnowHowApi.Services.Interfaces;
using KnowHowApi.Services.Utils;
using Microsoft.AspNetCore.SignalR;

namespace KnowHowApi.Services
{
    public class ChatService : IChatService
    {
        private const int TextoMaxLength = 2000;

        private readonly IChatRepository _chatRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IProfessorRepository _professorRepository;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IProfessorAlunoRepository _professorAlunoRepository;

        public ChatService(IChatRepository chatRepository, IUsuarioRepository usuarioRepository, IProfessorRepository professorRepository, IHubContext<ChatHub> hubContext, IProfessorAlunoRepository professorAlunoRepository)
        {
            _chatRepository = chatRepository;
            _usuarioRepository = usuarioRepository;
            _professorRepository = professorRepository;
            _hubContext = hubContext;
            _professorAlunoRepository = professorAlunoRepository;
        }

        public async Task<List<ConversaResumoDTO>> ListarConversas(int usuarioId)
        {
            var conversas = await _chatRepository.ListarConversasDoUsuario(usuarioId);
            if (conversas.Count == 0)
                return new List<ConversaResumoDTO>();

            var conversaIds = conversas.Select(c => c.Id).ToList();
            var ultimasMensagens = await _chatRepository.ObterUltimasMensagens(conversaIds);
            var naoLidasPorConversa = await _chatRepository.ContarNaoLidasPorConversa(conversaIds, usuarioId);

            var resumos = new List<ConversaResumoDTO>();
            foreach (var conversa in conversas)
            {
                var participanteEhProfessor = conversa.AlunoId == usuarioId;
                var participante = participanteEhProfessor ? conversa.Professor : conversa.Aluno;

                string? fotoUrl;
                if (participanteEhProfessor)
                {
                    var perfilProfessor = await _professorRepository.GetPerfilProfessorByUsuarioId(conversa.ProfessorId);
                    fotoUrl = FotoPerfilConverter.ToDataUri(perfilProfessor?.FotoPerfil, perfilProfessor?.FotoPerfilContentType);
                }
                else
                {
                    fotoUrl = FotoPerfilConverter.ToDataUri(participante.FotoPerfil, participante.FotoPerfilContentType);
                }

                ultimasMensagens.TryGetValue(conversa.Id, out var ultimaMensagem);
                naoLidasPorConversa.TryGetValue(conversa.Id, out var naoLidas);

                resumos.Add(MapParaResumo(
                    conversa.Id,
                    participante.Id,
                    participante.Nome,
                    fotoUrl,
                    ultimaMensagem,
                    naoLidas,
                    conversa.CriadaEm));
            }

            return resumos.OrderByDescending(r => r.UltimaMensagemEm).ToList();
        }

        public async Task<List<MensagemDTO>?> ListarMensagens(int conversaId, int usuarioId)
        {
            var conversa = await _chatRepository.GetConversaById(conversaId);
            if (!EhParticipante(conversa, usuarioId))
                return null;

            var mensagens = await _chatRepository.ListarMensagens(conversaId);
            return mensagens.Select(MapParaMensagemDTO).ToList();
        }

        public async Task<MensagemDTO?> EnviarMensagem(int conversaId, int usuarioId, EnviarMensagemRequestDTO request)
        {
            var conversa = await _chatRepository.GetConversaById(conversaId);
            if (!EhParticipante(conversa, usuarioId))
                return null;

            if (request == null || string.IsNullOrWhiteSpace(request.Texto))
                throw new DomainValidationException("Informe o texto da mensagem.");

            var texto = request.Texto.Trim();
            if (texto.Length > TextoMaxLength)
                throw new DomainValidationException($"A mensagem deve ter no máximo {TextoMaxLength} caracteres.");

            var mensagem = new Mensagem
            {
                ConversaId = conversaId,
                RemetenteId = usuarioId,
                Texto = texto,
                EnviadaEm = DateTime.UtcNow,
                Lida = false
            };

            mensagem = await _chatRepository.CriarMensagem(mensagem);
            var dto = MapParaMensagemDTO(mensagem);

            var outroParticipanteId = conversa!.AlunoId == usuarioId ? conversa.ProfessorId : conversa.AlunoId;
            await NotificarNovaMensagem(conversaId, usuarioId, outroParticipanteId, dto);

            return dto;
        }

        public async Task<ConversaResumoDTO> IniciarConversa(int alunoId, IniciarConversaRequestDTO request)
        {
            if (request == null || request.ProfessorId <= 0)
                throw new DomainValidationException("Informe o professor para iniciar a conversa.");

            var professor = await _usuarioRepository.GetUsuarioById(request.ProfessorId);
            if (professor == null || professor.TipoUsuario != TipoUsuario.Professor)
                throw new DomainValidationException("Professor não encontrado.");

            var conversa = await _chatRepository.GetConversaEntreAlunoEProfessor(alunoId, request.ProfessorId);
            conversa ??= await _chatRepository.CriarConversa(new Conversa
            {
                AlunoId = alunoId,
                ProfessorId = request.ProfessorId,
                CriadaEm = DateTime.UtcNow
            });

            var perfilProfessor = await _professorRepository.GetPerfilProfessorByUsuarioId(professor.Id);
            var fotoUrl = FotoPerfilConverter.ToDataUri(perfilProfessor?.FotoPerfil, perfilProfessor?.FotoPerfilContentType);

            var ultimas = await _chatRepository.ObterUltimasMensagens(new[] { conversa.Id });
            ultimas.TryGetValue(conversa.Id, out var ultimaMensagem);
            var contagens = await _chatRepository.ContarNaoLidasPorConversa(new[] { conversa.Id }, alunoId);
            contagens.TryGetValue(conversa.Id, out var naoLidas);

            return MapParaResumo(conversa.Id, professor.Id, professor.Nome, fotoUrl, ultimaMensagem, naoLidas, conversa.CriadaEm);
        }

        public async Task<ConversaResumoDTO> IniciarConversaComAluno(int professorId, IniciarConversaComAlunoRequestDTO request)
        {
            if (request == null || request.AlunoId <= 0)
                throw new DomainValidationException("Informe o aluno para iniciar a conversa.");

            // Só considera alunos (TipoUsuario.Aluno) que já estão na lista do professor.
            var alunoDaLista = await _professorAlunoRepository.GetAlunoDoProfessor(professorId, request.AlunoId);
            var aluno = alunoDaLista == null ? null : await _usuarioRepository.GetUsuarioById(request.AlunoId);
            if (aluno == null || aluno.TipoUsuario != TipoUsuario.Aluno)
                throw new DomainValidationException("Aluno não encontrado entre os seus alunos.");

            var conversa = await _chatRepository.GetConversaEntreAlunoEProfessor(aluno.Id, professorId);
            conversa ??= await _chatRepository.CriarConversa(new Conversa
            {
                AlunoId = aluno.Id,
                ProfessorId = professorId,
                CriadaEm = DateTime.UtcNow
            });

            var fotoUrl = FotoPerfilConverter.ToDataUri(aluno.FotoPerfil, aluno.FotoPerfilContentType);

            var ultimas = await _chatRepository.ObterUltimasMensagens(new[] { conversa.Id });
            ultimas.TryGetValue(conversa.Id, out var ultimaMensagem);
            var contagens = await _chatRepository.ContarNaoLidasPorConversa(new[] { conversa.Id }, professorId);
            contagens.TryGetValue(conversa.Id, out var naoLidas);

            return MapParaResumo(conversa.Id, aluno.Id, aluno.Nome, fotoUrl, ultimaMensagem, naoLidas, conversa.CriadaEm);
        }

        public async Task<bool?> MarcarComoLida(int conversaId, int usuarioId)
        {
            var conversa = await _chatRepository.GetConversaById(conversaId);
            if (!EhParticipante(conversa, usuarioId))
                return null;

            var atualizadas = await _chatRepository.MarcarMensagensComoLidas(conversaId, usuarioId);
            if (atualizadas > 0)
            {
                var outroParticipanteId = conversa!.AlunoId == usuarioId ? conversa.ProfessorId : conversa.AlunoId;
                await NotificarMensagensLidas(conversaId, usuarioId, outroParticipanteId);
            }

            return true;
        }

        private async Task NotificarNovaMensagem(int conversaId, int remetenteId, int outroParticipanteId, MensagemDTO mensagem)
        {
            var payload = new { conversaId = conversaId.ToString(), mensagem };
            await _hubContext.Clients.Group(ChatHub.GrupoUsuario(remetenteId)).SendAsync("NovaMensagem", payload);
            await _hubContext.Clients.Group(ChatHub.GrupoUsuario(outroParticipanteId)).SendAsync("NovaMensagem", payload);
        }

        private async Task NotificarMensagensLidas(int conversaId, int leitorId, int outroParticipanteId)
        {
            var payload = new { conversaId = conversaId.ToString(), leitorId };
            await _hubContext.Clients.Group(ChatHub.GrupoUsuario(leitorId)).SendAsync("MensagensLidas", payload);
            await _hubContext.Clients.Group(ChatHub.GrupoUsuario(outroParticipanteId)).SendAsync("MensagensLidas", payload);
        }

        private static bool EhParticipante(Conversa? conversa, int usuarioId)
        {
            return conversa != null && (conversa.AlunoId == usuarioId || conversa.ProfessorId == usuarioId);
        }

        private static MensagemDTO MapParaMensagemDTO(Mensagem mensagem)
        {
            return new MensagemDTO
            {
                Id = mensagem.Id.ToString(),
                RemetenteId = mensagem.RemetenteId,
                Texto = mensagem.Texto,
                EnviadaEm = mensagem.EnviadaEm,
                Lida = mensagem.Lida
            };
        }

        private static ConversaResumoDTO MapParaResumo(int conversaId, int participanteId, string participanteNome, string? participanteFotoUrl, Mensagem? ultimaMensagem, int naoLidas, DateTime criadaEm)
        {
            return new ConversaResumoDTO
            {
                Id = conversaId.ToString(),
                ParticipanteId = participanteId,
                ParticipanteNome = participanteNome,
                ParticipanteFotoUrl = participanteFotoUrl,
                UltimaMensagem = ultimaMensagem?.Texto ?? string.Empty,
                UltimaMensagemEm = ultimaMensagem?.EnviadaEm ?? criadaEm,
                NaoLidas = naoLidas
            };
        }
    }
}
