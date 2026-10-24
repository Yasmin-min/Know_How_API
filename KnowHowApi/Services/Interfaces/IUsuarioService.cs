using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Models;

namespace KnowHowApi.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<Usuario> Registrar(RegisterUsuarioDTO request);
        Task<LoginResponseDTO> ValidateLogin(LoginRequestDTO request);
        Task SolicitarRecuperacaoSenha(string email);
        Task<string> ConfirmarCodigoRecuperacao(string email, string codigo);
        Task RedefinirSenha(string email, string tokenRedefinicao, string novaSenha);
        Task AlterarSenha(int usuarioId, string senhaAtual, string novaSenha);
        Task<(byte[] Bytes, string ContentType)?> ObterFoto(int usuarioId);
    }
}
