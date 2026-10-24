namespace KnowHowApi.Domain.DTOs
{
    // Body de POST /Professor/aulas-agendadas. O professor vem do token, nunca do body.
    public class CriarAulaAgendadaRequestDTO
    {
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public int? AlunoId { get; set; }
        public int? AulaId { get; set; }
        public DateTime? Inicio { get; set; }
        public int? DuracaoMinutos { get; set; }
    }
}
