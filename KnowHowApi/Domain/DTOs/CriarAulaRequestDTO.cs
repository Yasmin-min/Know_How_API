namespace KnowHowApi.Domain.DTOs
{
    public class CriarAulaRequestDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? Materia { get; set; }
        public decimal? Valor { get; set; }
        public int? DuracaoMinutos { get; set; }
        public string? Nivel { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
