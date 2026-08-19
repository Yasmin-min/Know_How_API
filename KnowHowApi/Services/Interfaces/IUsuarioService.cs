using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Models;

namespace KnowHowApi.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<Usuario> Registrar(RegisterUsuarioDTO request);
        Task<LoginResponseDTO> ValidateLogin(LoginRequestDTO request);
        Task<List<Usuario>> ListarTodosOsUsuarios();
        Task<Usuario> EditarUsuario(int usuarioId, UsuarioDTO userEditDto);
        Task SolicitarRecuperacaoSenha(string email);
        Task<string> ConfirmarCodigoRecuperacao(string email, string codigo);
        Task RedefinirSenha(string email, string tokenRedefinicao, string novaSenha);
    }
}
