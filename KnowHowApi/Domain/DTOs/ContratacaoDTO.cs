namespace KnowHowApi.Domain.DTOs
{
    // Tipo: "avulsa" | "pacote" | "mensal". StatusPagamento: "pendente" | "pago".
    // Inicio/Fim são datas sem horário (yyyy-mm-dd); PagoEm/CriadaEm em UTC.
    public class ContratacaoDTO
    {
        public string Id { get; set; }
        public int AlunoId { get; set; }
        public string AlunoNome { get; set; }
        public string Tipo { get; set; }
        public string? OfertaId { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public int TotalAulas { get; set; }
        public int AulasUsadas { get; set; }
        public DateOnly Inicio { get; set; }
        public DateOnly? Fim { get; set; }
        public string StatusPagamento { get; set; }
        public DateTime? PagoEm { get; set; }
        public DateTime CriadaEm { get; set; }
    }
}
