namespace KnowHowApi.Domain.DTOs
{
    public class PacoteDTO
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int QuantidadeAulas { get; set; }
        public int DuracaoMinutos { get; set; }
        public decimal ValorTotal { get; set; }
        public int ValidadeDias { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
