namespace KnowHowApi.Domain.DTOs
{
    // Status esperados pelo frontend: "ativa" | "pausada" | "rascunho".
    // Usado tanto na resposta de POST /Professor/aulas quanto em minhasAulas do GET /Professor/dashboard.
    public class ProfessorAulaResponseDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string? Materia { get; set; }
        public decimal? Valor { get; set; }
        public int? DuracaoMinutos { get; set; }
        public string? Nivel { get; set; }
        public string Status { get; set; }
        public int TotalAlunos { get; set; }
        public DateTime CriadaEm { get; set; }
    }
}
