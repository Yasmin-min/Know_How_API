namespace KnowHowApi.Domain.DTOs
{
    // Body de PUT /Aluno/avaliacoes/{professorId}. Cria ou substitui a avaliação do aluno.
    public class SalvarAvaliacaoRequestDTO
    {
        public int Nota { get; set; }
        public string? Comentario { get; set; }
    }
}
