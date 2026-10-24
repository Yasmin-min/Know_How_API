using KnowHowApi.Domain.DTOs;

namespace KnowHowApi.Services.Interfaces
{
    public interface IAlunoService
    {
        Task<AlunoPerfilResponseDTO?> ObterPerfil(int usuarioId);
        Task<AlunoPerfilResponseDTO?> AtualizarPerfil(int usuarioId, AlunoPerfilRequestDTO request);
    }
}
