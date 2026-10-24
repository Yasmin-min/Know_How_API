using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services.Interfaces;

namespace KnowHowApi.Services
{
    public class ChamadaService : IChamadaService
    {
        // A sala abre um pouco antes para testar câmera/microfone e fica aberta um tempo
        // depois do fim, caso a aula se estenda.
        public const int MinutosAntesDeAbrir = 15;
        public const int MinutosDepoisDoFim = 30;

        private readonly IAulaAgendadaRepository _aulaAgendadaRepository;
        private readonly ICredenciaisTurnService _credenciaisTurnService;

        public ChamadaService(IAulaAgendadaRepository aulaAgendadaRepository, ICredenciaisTurnService credenciaisTurnService)
        {
            _aulaAgendadaRepository = aulaAgendadaRepository;
            _credenciaisTurnService = credenciaisTurnService;
        }

        public async Task<ChamadaAcessoDTO> ObterAcesso(int usuarioId, Guid aulaId)
        {
            var aula = await GetAulaDoParticipante(usuarioId, aulaId);
            var souProfessor = aula.ProfessorId == usuarioId;
            var outro = souProfessor ? aula.Aluno : aula.Professor;
            var situacao = CalcularSituacao(aula, DateTime.UtcNow);

            return new ChamadaAcessoDTO
            {
                AulaId = aula.Id.ToString(),
                Titulo = aula.Titulo,
                Inicio = aula.Inicio,
                Fim = aula.Fim,
                AbreEm = AbreEm(aula),
                FechaEm = FechaEm(aula),
                Situacao = situacao,
                SouProfessor = souProfessor,
                Participante = new ParticipanteChamadaDTO
                {
                    Id = souProfessor ? aula.AlunoId : aula.ProfessorId,
                    Nome = outro?.Nome ?? string.Empty
                },
                IceServers = situacao == ChamadaAcessoDTO.SituacaoAberta
                    ? await _credenciaisTurnService.GerarIceServers()
                    : new List<IceServerDTO>()
            };
        }

        public async Task ValidarEntrada(int usuarioId, Guid aulaId)
        {
            var aula = await GetAulaDoParticipante(usuarioId, aulaId);
            switch (CalcularSituacao(aula, DateTime.UtcNow))
            {
                case ChamadaAcessoDTO.SituacaoCancelada:
                    throw new ForbiddenException("Esta aula foi cancelada.");
                case ChamadaAcessoDTO.SituacaoAguardando:
                    throw new ForbiddenException($"A sala abre {MinutosAntesDeAbrir} minutos antes do início da aula.");
                case ChamadaAcessoDTO.SituacaoEncerrada:
                    throw new ForbiddenException("Esta aula já terminou.");
            }
        }

        private async Task<AulaAgendada> GetAulaDoParticipante(int usuarioId, Guid aulaId)
        {
            var aula = await _aulaAgendadaRepository.GetByIdComParticipantes(aulaId);
            if (aula == null || (aula.ProfessorId != usuarioId && aula.AlunoId != usuarioId))
                throw new NotFoundException("Aula não encontrada.");
            return aula;
        }

        private static DateTime AbreEm(AulaAgendada aula) => aula.Inicio.AddMinutes(-MinutosAntesDeAbrir);

        private static DateTime FechaEm(AulaAgendada aula) => aula.Fim.AddMinutes(MinutosDepoisDoFim);

        private static string CalcularSituacao(AulaAgendada aula, DateTime agora)
        {
            if (aula.Status == AulaAgendada.StatusCancelada)
                return ChamadaAcessoDTO.SituacaoCancelada;
            if (agora < AbreEm(aula))
                return ChamadaAcessoDTO.SituacaoAguardando;
            if (agora > FechaEm(aula))
                return ChamadaAcessoDTO.SituacaoEncerrada;
            return ChamadaAcessoDTO.SituacaoAberta;
        }
    }
}
