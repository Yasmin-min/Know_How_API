using KnowHowApi.Domain.Enum;

namespace KnowHowApi.Domain.DTOs
{
    public class RegisterUsuarioDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public TipoUsuario TipoUsuario { get; set; }
        public DateTime DataNascimento { get; set; }
        public string? Cpf { get; set; }
        public string? AreaInteresse { get; set; }
        public string? AreaEspecialidade { get; set; }
        public string? AnosExperiencia { get; set; }
        public string? MiniBiografia { get; set; }
        public string? FotoPerfilBase64 { get; set; }
    }
}
