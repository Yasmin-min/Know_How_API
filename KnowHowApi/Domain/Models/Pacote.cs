namespace KnowHowApi.Domain.Models
{
    // Pacote de aulas vendido pelo professor (ex.: 8 aulas de 1h válidas por 60 dias).
    public class Pacote
    {
        public Guid Id { get; set; }

        public int ProfessorId { get; set; }
        public Usuario Professor { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int QuantidadeAulas { get; set; }
        public int DuracaoMinutos { get; set; }
        public decimal ValorTotal { get; set; }
        public int ValidadeDias { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
