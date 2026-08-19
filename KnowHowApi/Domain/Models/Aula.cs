namespace KnowHowApi.Domain.Models
{
    public class Aula
    {
        public int Id { get; set; }

        public int ProfessorId { get; set; }
        public Usuario Professor { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;

        public int? AreaInteresseId { get; set; }
        public AreaInteresse? AreaInteresse { get; set; }

        public decimal? Valor { get; set; }
        public int? DuracaoMinutos { get; set; }
        public string? Nivel { get; set; }

        public string Status { get; set; }
        public DateTime CriadaEm { get; set; }
    }
}
