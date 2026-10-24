namespace KnowHowApi.Domain.DTOs
{
    // Body de POST /Professor/contratacoes. O professor vem do token, nunca do body.
    // Aula avulsa: OfertaId e Fim nulos. Pacote/mensal: OfertaId e Fim obrigatórios.
    public class CriarContratacaoRequestDTO
    {
        public int? AlunoId { get; set; }
        public string? Tipo { get; set; }
        public string? OfertaId { get; set; }
        public string? Descricao { get; set; }
        public decimal? Valor { get; set; }
        public int? TotalAulas { get; set; }
        public DateOnly? Inicio { get; set; }
        public DateOnly? Fim { get; set; }
        public bool Pago { get; set; }
    }
}
