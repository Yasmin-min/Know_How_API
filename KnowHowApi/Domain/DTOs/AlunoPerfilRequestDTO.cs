namespace KnowHowApi.Domain.DTOs
{
    public class AlunoPerfilRequestDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public string AreaInteresse { get; set; } = string.Empty;
        public string? FotoPerfilBase64 { get; set; }
    }
}
