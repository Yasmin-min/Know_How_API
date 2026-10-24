namespace KnowHowApi.Domain.Models
{
    // Configuração de preços do professor (uma por professor). O valor da hora fica em
    // PerfilProfessor.ValorHora; aqui ficam só os preços por duração e a aula experimental.
    public class ConfiguracaoPrecos
    {
        public const string ExperimentalNenhuma = "nenhuma";
        public const string ExperimentalGratis = "gratis";
        public const string ExperimentalDesconto = "desconto";

        public int Id { get; set; }

        public int ProfessorId { get; set; }
        public Usuario Professor { get; set; }

        // Gravado como jsonb. Durações ausentes usam o valor proporcional à hora.
        public List<PrecoPorDuracao> PrecosPorDuracao { get; set; } = new();

        public string AulaExperimentalTipo { get; set; } = ExperimentalNenhuma;
        // Só preenchido quando AulaExperimentalTipo = "desconto".
        public int? AulaExperimentalPercentualDesconto { get; set; }
        public int AulaExperimentalDuracaoMinutos { get; set; } = 30;

        public DateTime AtualizadaEm { get; set; }
    }

    public class PrecoPorDuracao
    {
        public int DuracaoMinutos { get; set; }
        public decimal Valor { get; set; }
    }
}
