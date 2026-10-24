namespace KnowHowApi.Domain.DTOs
{
    // GET /Professor/{professorId}/avaliacoes e GET /Professor/avaliacoes.
    public class AvaliacoesProfessorDTO
    {
        public ResumoAvaliacoesDTO Resumo { get; set; } = new();
        // Da mais recente (criação ou edição) para a mais antiga.
        public List<AvaliacaoDTO> Avaliacoes { get; set; } = new();
        // Avaliação do aluno logado para este professor; null se não avaliou ou se quem pede é professor.
        public AvaliacaoDTO? MinhaAvaliacao { get; set; }
    }
}
