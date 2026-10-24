namespace KnowHowApi.Domain.DTOs
{
    // Body de POST /Professor/planos-mensais e PUT /Professor/planos-mensais/{id}.
    public class SalvarPlanoMensalRequestDTO
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public int? AulasPorSemana { get; set; }
        public int? DuracaoMinutos { get; set; }
        public decimal? ValorMensal { get; set; }
        public bool? Ativo { get; set; }
    }
}
