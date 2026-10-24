using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Exceptions;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using KnowHowApi.Services.Interfaces;

namespace KnowHowApi.Services
{
    public class AvaliacaoService : IAvaliacaoService
    {
        private const int NotaMinima = 1;
        private const int NotaMaxima = 5;
        private const int ComentarioMaxLength = 500;

        private readonly IAvaliacaoRepository _avaliacaoRepository;

        public AvaliacaoService(IAvaliacaoRepository avaliacaoRepository)
        {
            _avaliacaoRepository = avaliacaoRepository;
        }

        public async Task<AvaliacoesProfessorDTO> ListarDoProfessor(int professorId, int? alunoLogadoId)
        {
            if (!await _avaliacaoRepository.ProfessorExiste(professorId))
                throw new NotFoundException("Professor não encontrado.");

            var avaliacoes = await _avaliacaoRepository.ListarPorProfessor(professorId);
            var dtos = avaliacoes.Select(ParaDTO).ToList();

            return new AvaliacoesProfessorDTO
            {
                Resumo = CalcularResumo(avaliacoes),
                Avaliacoes = dtos,
                MinhaAvaliacao = alunoLogadoId == null ? null : dtos.FirstOrDefault(a => a.AlunoId == alunoLogadoId)
            };
        }

        public async Task<AvaliacaoDTO> Salvar(int alunoId, int professorId, SalvarAvaliacaoRequestDTO request)
        {
            var nota = request?.Nota ?? 0;
            if (nota < NotaMinima || nota > NotaMaxima)
                throw new DomainValidationException($"A nota deve ser de {NotaMinima} a {NotaMaxima} estrelas.");

            var comentario = (request?.Comentario ?? string.Empty).Trim();
            if (comentario.Length > ComentarioMaxLength)
                throw new DomainValidationException($"O comentário deve ter no máximo {ComentarioMaxLength} caracteres.");

            if (!await _avaliacaoRepository.ProfessorExiste(professorId))
                throw new NotFoundException("Professor não encontrado.");

            var agora = DateTime.UtcNow;
            if (!await _avaliacaoRepository.AlunoTeveAulaComProfessor(professorId, alunoId, agora))
                throw new ForbiddenException("Você só pode avaliar um professor depois de ter uma aula com ele.");

            var avaliacao = await _avaliacaoRepository.Salvar(professorId, alunoId, nota, comentario, agora);
            await _avaliacaoRepository.AtualizarResumoDoPerfil(professorId);
            return ParaDTO(avaliacao);
        }

        public async Task Excluir(int alunoId, int professorId)
        {
            if (!await _avaliacaoRepository.Remover(professorId, alunoId))
                throw new NotFoundException("Avaliação não encontrada.");

            await _avaliacaoRepository.AtualizarResumoDoPerfil(professorId);
        }

        private static ResumoAvaliacoesDTO CalcularResumo(List<Avaliacao> avaliacoes)
        {
            var resumo = new ResumoAvaliacoesDTO { Total = avaliacoes.Count };
            foreach (var avaliacao in avaliacoes)
                resumo.Distribuicao[avaliacao.Nota - 1]++;

            if (avaliacoes.Count > 0)
                resumo.Media = Math.Round((decimal)avaliacoes.Average(a => a.Nota), 1);

            return resumo;
        }

        private static AvaliacaoDTO ParaDTO(Avaliacao avaliacao)
        {
            return new AvaliacaoDTO
            {
                Id = avaliacao.Id,
                ProfessorId = avaliacao.ProfessorId,
                AlunoId = avaliacao.AlunoId,
                AlunoNome = avaliacao.Aluno?.Nome ?? string.Empty,
                Nota = avaliacao.Nota,
                Comentario = avaliacao.Comentario,
                CriadaEm = avaliacao.CriadaEm,
                AtualizadaEm = avaliacao.AtualizadaEm
            };
        }
    }
}
