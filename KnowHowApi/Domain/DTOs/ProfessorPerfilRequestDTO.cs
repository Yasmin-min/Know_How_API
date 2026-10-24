namespace KnowHowApi.Domain.DTOs
{
    public class ProfessorPerfilRequestDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public string AreaEspecialidade { get; set; } = string.Empty;
        public string AnosExperiencia { get; set; } = string.Empty;
        public string MiniBiografia { get; set; } = string.Empty;
        public decimal ValorHora { get; set; }
        public bool Disponivel { get; set; }
        public string? FotoPerfilBase64 { get; set; }
    }
}
