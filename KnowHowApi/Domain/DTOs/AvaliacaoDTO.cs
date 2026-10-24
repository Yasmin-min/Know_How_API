namespace KnowHowApi.Domain.DTOs
{
    public class AvaliacaoDTO
    {
        public int Id { get; set; }
        public int ProfessorId { get; set; }
        public int AlunoId { get; set; }
        public string AlunoNome { get; set; } = string.Empty;
        public int Nota { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public DateTime CriadaEm { get; set; }
        public DateTime? AtualizadaEm { get; set; }
    }
}
