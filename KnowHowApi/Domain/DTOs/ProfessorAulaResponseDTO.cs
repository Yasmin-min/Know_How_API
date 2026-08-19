namespace KnowHowApi.Domain.DTOs
{
    // Status esperados pelo frontend: "ativa" | "pausada" | "rascunho".
    // Ainda sem entidade de Aula no banco; lista sempre vazia nesta primeira versão.
    public class ProfessorAulaResponseDTO
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string Materia { get; set; }
        public int TotalAlunos { get; set; }
        public string Status { get; set; }
    }
}
