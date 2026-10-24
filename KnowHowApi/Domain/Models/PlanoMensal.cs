namespace KnowHowApi.Domain.Models
{
    // Mensalidade oferecida pelo professor (ex.: 2 aulas por semana de 1h).
    public class PlanoMensal
    {
        public Guid Id { get; set; }

        public int ProfessorId { get; set; }
        public Usuario Professor { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int AulasPorSemana { get; set; }
        public int DuracaoMinutos { get; set; }
        public decimal ValorMensal { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
