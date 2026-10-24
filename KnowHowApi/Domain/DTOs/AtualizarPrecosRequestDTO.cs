namespace KnowHowApi.Domain.DTOs
{
    // Body de PUT /Professor/precos. O ValorHora também atualiza o perfil do professor.
    public class AtualizarPrecosRequestDTO
    {
        public decimal? ValorHora { get; set; }
        public List<PrecoPorDuracaoDTO>? PrecosPorDuracao { get; set; }
        public AulaExperimentalDTO? AulaExperimental { get; set; }
    }
}
