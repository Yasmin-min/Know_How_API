namespace KnowHowApi.Domain.DTOs
{
    // Body de PUT /Professor/aulas-agendadas/{id}. O aluno não muda: para trocar de aluno, cancele e agende outra aula.
    public class EditarAulaAgendadaRequestDTO
    {
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public int? AulaId { get; set; }
        public DateTime? Inicio { get; set; }
        public int? DuracaoMinutos { get; set; }
    }
}
