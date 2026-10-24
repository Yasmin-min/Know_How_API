namespace KnowHowApi.Domain.DTOs
{
    // GET/PUT /Professor/precos e parte de GET /Professor/{professorId}/ofertas.
    // ValorHora vem do perfil do professor; null quando ainda não foi definido.
    public class ConfiguracaoPrecosDTO
    {
        public decimal? ValorHora { get; set; }
        public List<PrecoPorDuracaoDTO> PrecosPorDuracao { get; set; } = new();
        public AulaExperimentalDTO AulaExperimental { get; set; } = new();
    }

    public class PrecoPorDuracaoDTO
    {
        public int? DuracaoMinutos { get; set; }
        public decimal? Valor { get; set; }
    }

    // Tipo: "nenhuma" | "gratis" | "desconto". PercentualDesconto só quando Tipo = "desconto".
    public class AulaExperimentalDTO
    {
        public string? Tipo { get; set; }
        public int? PercentualDesconto { get; set; }
        public int? DuracaoMinutos { get; set; }
    }
}
