using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public TipoUsuario TipoUsuario { get; set; }

        public UsuarioDTO()
        {
        }

        public UsuarioDTO(Usuario usuario)
        {
            Id = usuario.Id;
            Nome = usuario.Nome;
            Email = usuario.Email;
            TipoUsuario = usuario.TipoUsuario;
        }
    }
}
