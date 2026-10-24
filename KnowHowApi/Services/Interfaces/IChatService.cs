using KnowHowApi.Domain.DTOs;

namespace KnowHowApi.Services.Interfaces
{
    public interface IChatService
    {
        Task<List<ConversaResumoDTO>> ListarConversas(int usuarioId);
        Task<List<MensagemDTO>?> ListarMensagens(int conversaId, int usuarioId);
        Task<MensagemDTO?> EnviarMensagem(int conversaId, int usuarioId, EnviarMensagemRequestDTO request);
        Task<ConversaResumoDTO> IniciarConversa(int alunoId, IniciarConversaRequestDTO request);
        Task<ConversaResumoDTO> IniciarConversaComAluno(int professorId, IniciarConversaComAlunoRequestDTO request);
        Task<bool?> MarcarComoLida(int conversaId, int usuarioId);
    }
}
