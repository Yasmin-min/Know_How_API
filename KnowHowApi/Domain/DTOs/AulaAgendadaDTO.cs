namespace KnowHowApi.Domain.DTOs
{
    // Status: "agendada" | "cancelada". Datas sempre em UTC (serializadas com "Z").
    public class AulaAgendadaDTO
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public int AlunoId { get; set; }
        public string AlunoNome { get; set; }
        public int? AulaId { get; set; }
        public DateTime Inicio { get; set; }
        public int DuracaoMinutos { get; set; }
        public string Status { get; set; }
        public DateTime CriadaEm { get; set; }
    }
}
