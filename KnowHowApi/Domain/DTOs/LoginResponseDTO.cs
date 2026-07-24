using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.DTOs
{
    public class LoginResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public TipoUsuario TipoUsuario { get; set; }
        public string Token { get; set; }

        public LoginResponseDTO()
        {
        }

        public LoginResponseDTO(Usuario usuario, string token)
        {
            Id = usuario.Id;
            Email = usuario.Email;
            Nome = usuario.Nome;
            TipoUsuario = usuario.TipoUsuario;
            Token = token;
        }
    }
}
