namespace KnowHowApi.Domain.DTOs
{
    // Status esperados pelo frontend: "confirmada" | "agendada" | "cancelada".
    // Ainda sem entidade de Aula no banco; lista sempre vazia nesta primeira versão.
    public class ProfessorDashboardProximaAulaDTO
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }
        public int TotalAlunos { get; set; }
        public string Status { get; set; }
    }
}
