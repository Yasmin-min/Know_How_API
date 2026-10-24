using KnowHowApi.Domain.DTOs;

namespace KnowHowApi.Services.Interfaces
{
    public interface IChamadaService
    {
        // NotFound se a aula não existir ou o usuário não for o professor nem o aluno dela.
        Task<ChamadaAcessoDTO> ObterAcesso(int usuarioId, Guid aulaId);
        // Para o hub: garante que o usuário participa da aula e que a sala está aberta agora.
        Task ValidarEntrada(int usuarioId, Guid aulaId);
    }
}
