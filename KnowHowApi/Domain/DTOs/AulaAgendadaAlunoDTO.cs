namespace KnowHowApi.Domain.DTOs
{
    // GET /Aluno/aulas-agendadas: as aulas do aluno logado, com o professor no lugar do aluno.
    public class AulaAgendadaAlunoDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int ProfessorId { get; set; }
        public string ProfessorNome { get; set; } = string.Empty;
        public DateTime Inicio { get; set; }
        public int DuracaoMinutos { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
