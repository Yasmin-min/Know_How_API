using KnowHowApi.Domain.Enum;

namespace KnowHowApi.Domain.DTOs
{
    public class RegisterUsuarioDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public TipoUsuario TipoUsuario { get; set; }
    }
}
