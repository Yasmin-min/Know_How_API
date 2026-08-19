namespace KnowHowApi.Domain.DTOs
{
    public class ProfessorDashboardResponseDTO
    {
        public string? Especialidade { get; set; }
        public ProfessorDashboardIndicadoresDTO Indicadores { get; set; }
        public List<ProfessorDashboardProximaAulaDTO> ProximasAulas { get; set; }
        public List<ProfessorDashboardAtividadeDTO> AtividadesRecentes { get; set; }
        public List<ProfessorAulaResponseDTO> MinhasAulas { get; set; }
    }
}
