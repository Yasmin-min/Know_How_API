using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IChatRepository
    {
        Task<Conversa?> GetConversaEntreAlunoEProfessor(int alunoId, int professorId);
        Task<Conversa?> GetConversaById(int conversaId);
        Task<Conversa> CriarConversa(Conversa conversa);
        Task<List<Conversa>> ListarConversasDoUsuario(int usuarioId);
        Task<Dictionary<int, Mensagem>> ObterUltimasMensagens(IEnumerable<int> conversaIds);
        Task<Dictionary<int, int>> ContarNaoLidasPorConversa(IEnumerable<int> conversaIds, int usuarioId);
        Task<List<Mensagem>> ListarMensagens(int conversaId);
        Task<Mensagem> CriarMensagem(Mensagem mensagem);
        Task<int> MarcarMensagensComoLidas(int conversaId, int usuarioId);
    }
};
