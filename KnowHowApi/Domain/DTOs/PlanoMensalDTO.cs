namespace KnowHowApi.Domain.DTOs
{
    public class PlanoMensalDTO
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int AulasPorSemana { get; set; }
        public int DuracaoMinutos { get; set; }
        public decimal ValorMensal { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
