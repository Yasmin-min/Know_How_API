using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces 
{
    public interface IUsuarioRepository
    {
        Task<Usuario> CriarUsuario(Usuario usuario);
        Task<Usuario?> GetUsuarioByEmail(string email);
        Task<Usuario?> GetUsuarioByEmailIgnorandoCaixa(string emailMinusculo);
        Task<Usuario?> GetUsuarioByCpf(string cpf);
        Task<Usuario?> GetUsuarioById(int UsuarioId);
        Task<Usuario> Update(Usuario userEdit); 
    }
};