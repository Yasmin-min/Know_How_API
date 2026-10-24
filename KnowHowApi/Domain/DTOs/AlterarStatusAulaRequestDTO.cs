namespace KnowHowApi.Domain.DTOs
{
    // Body de PUT /Professor/aulas/{id}/status: "ativa" | "pausada" | "rascunho".
    public class AlterarStatusAulaRequestDTO
    {
        public string? Status { get; set; }
    }
}
