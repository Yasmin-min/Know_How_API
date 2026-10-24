namespace KnowHowApi.Domain.DTOs
{
    public class AlunoPerfilResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public string? AreaInteresse { get; set; }
        public string? FotoPerfilBase64 { get; set; }
    }
}
