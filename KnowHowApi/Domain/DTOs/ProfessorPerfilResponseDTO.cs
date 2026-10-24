namespace KnowHowApi.Domain.DTOs
{
    public class ProfessorPerfilResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string? Cpf { get; set; }
        public DateTime DataNascimento { get; set; }
        public string? AreaEspecialidade { get; set; }
        public string? AnosExperiencia { get; set; }
        public string? MiniBiografia { get; set; }
        public decimal ValorHora { get; set; }
        public bool Disponivel { get; set; }
        public string? FotoPerfilBase64 { get; set; }
    }
}
