using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowHowApi.Data.Repositories
{
    public class ChatRepository : BaseRepository, IChatRepository
    {
        public ChatRepository(Context context) : base(context)
        {
        }

        public async Task<Conversa?> GetConversaEntreAlunoEProfessor(int alunoId, int professorId)
        {
            return await _context.Conversas
                .FirstOrDefaultAsync(c => c.AlunoId == alunoId && c.ProfessorId == professorId);
        }

        public async Task<Conversa?> GetConversaById(int conversaId)
        {
            return await _context.Conversas
                .Include(c => c.Aluno)
                .Include(c => c.Professor)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == conversaId);
        }

        public async Task<Conversa> CriarConversa(Conversa conversa)
        {
            _context.Conversas.Add(conversa);
            await _context.SaveChangesAsync();
            return conversa;
        }

        public async Task<List<Conversa>> ListarConversasDoUsuario(int usuarioId)
        {
            return await _context.Conversas
                .Include(c => c.Aluno)
                .Include(c => c.Professor)
                .AsNoTracking()
                .Where(c => c.AlunoId == usuarioId || c.ProfessorId == usuarioId)
                .ToListAsync();
        }

        public async Task<Dictionary<int, Mensagem>> ObterUltimasMensagens(IEnumerable<int> conversaIds)
        {
            var ultimas = await _context.Mensagens
                .AsNoTracking()
                .Where(m => conversaIds.Contains(m.ConversaId))
                .GroupBy(m => m.ConversaId)
                .Select(g => g.OrderByDescending(m => m.EnviadaEm).First())
                .ToListAsync();

            return ultimas.ToDictionary(m => m.ConversaId);
        }

        public async Task<Dictionary<int, int>> ContarNaoLidasPorConversa(IEnumerable<int> conversaIds, int usuarioId)
        {
            var contagens = await _context.Mensagens
                .AsNoTracking()
                .Where(m => conversaIds.Contains(m.ConversaId) && m.RemetenteId != usuarioId && !m.Lida)
                .GroupBy(m => m.ConversaId)
                .Select(g => new { ConversaId = g.Key, Total = g.Count() })
                .ToListAsync();

            return contagens.ToDictionary(c => c.ConversaId, c => c.Total);
        }

        public async Task<List<Mensagem>> ListarMensagens(int conversaId)
        {
            return await _context.Mensagens
                .AsNoTracking()
                .Where(m => m.ConversaId == conversaId)
                .OrderBy(m => m.EnviadaEm)
                .ToListAsync();
        }

        public async Task<Mensagem> CriarMensagem(Mensagem mensagem)
        {
            _context.Mensagens.Add(mensagem);
            await _context.SaveChangesAsync();
            return mensagem;
        }

        public async Task<int> MarcarMensagensComoLidas(int conversaId, int usuarioId)
        {
            return await _context.Mensagens
                .Where(m => m.ConversaId == conversaId && m.RemetenteId != usuarioId && !m.Lida)
                .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.Lida, true));
        }
    }
};
