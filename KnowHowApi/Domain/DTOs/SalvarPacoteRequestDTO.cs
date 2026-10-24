namespace KnowHowApi.Domain.DTOs
{
    // Body de POST /Professor/pacotes e PUT /Professor/pacotes/{id}.
    public class SalvarPacoteRequestDTO
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public int? QuantidadeAulas { get; set; }
        public int? DuracaoMinutos { get; set; }
        public decimal? ValorTotal { get; set; }
        public int? ValidadeDias { get; set; }
        public bool? Ativo { get; set; }
    }
}
