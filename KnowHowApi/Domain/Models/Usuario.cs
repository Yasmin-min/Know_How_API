using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Enum;

namespace KnowHowApi.Domain.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string SenhaHash { get; set; }
        public TipoUsuario TipoUsuario { get; set; }

        public Usuario()
        {
        }

        public Usuario(UsuarioDTO usuarioDTO)
        {
            Id = usuarioDTO.Id;
            Nome = usuarioDTO.Nome;
            Email = usuarioDTO.Email;
            TipoUsuario = usuarioDTO.TipoUsuario;
        }
    }
}
