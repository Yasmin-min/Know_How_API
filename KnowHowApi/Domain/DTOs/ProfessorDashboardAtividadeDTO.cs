namespace KnowHowApi.Domain.DTOs
{
    // Tipos esperados pelo frontend: "avaliacao" | "novoAluno" | "mensagem".
    // Ainda sem entidade de Atividade/Avaliação individual no banco; lista sempre vazia nesta primeira versão.
    public class ProfessorDashboardAtividadeDTO
    {
        public string Id { get; set; }
        public string Tipo { get; set; }
        public string NomePessoa { get; set; }
        public string? AvatarUrl { get; set; }
        public string Titulo { get; set; }
        public string Detalhe { get; set; }
        public DateTime DataHora { get; set; }
        public int? AvaliacaoEstrelas { get; set; }
    }
}
