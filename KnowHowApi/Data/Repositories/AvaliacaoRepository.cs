using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowHowApi.Data.Repositories
{
    public class AvaliacaoRepository : BaseRepository, IAvaliacaoRepository
    {
        public AvaliacaoRepository(Context context) : base(context)
        {
        }

        public async Task<bool> ProfessorExiste(int professorId)
        {
            return await _context.PerfilProfessores.AnyAsync(p => p.UsuarioId == professorId);
        }

        // Basta uma aula não cancelada que já tenha começado.
        public async Task<bool> AlunoTeveAulaComProfessor(int professorId, int alunoId, DateTime agora)
        {
            return await _context.AulasAgendadas.AnyAsync(a =>
                a.ProfessorId == professorId
                && a.AlunoId == alunoId
                && a.Status == AulaAgendada.StatusAgendada
                && a.Inicio <= agora);
        }

        public async Task<List<Avaliacao>> ListarPorProfessor(int professorId)
        {
            return await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.ProfessorId == professorId)
                .OrderByDescending(a => a.AtualizadaEm ?? a.CriadaEm)
                .Select(a => new Avaliacao
                {
                    Id = a.Id,
                    ProfessorId = a.ProfessorId,
                    AlunoId = a.AlunoId,
                    Aluno = new Usuario { Id = a.AlunoId, Nome = a.Aluno.Nome },
                    Nota = a.Nota,
                    Comentario = a.Comentario,
                    CriadaEm = a.CriadaEm,
                    AtualizadaEm = a.AtualizadaEm
                })
                .ToListAsync();
        }

        public async Task<Avaliacao> Salvar(int professorId, int alunoId, int nota, string comentario, DateTime agora)
        {
            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Aluno)
                .FirstOrDefaultAsync(a => a.ProfessorId == professorId && a.AlunoId == alunoId);

            if (avaliacao == null)
            {
                avaliacao = new Avaliacao
                {
                    ProfessorId = professorId,
                    AlunoId = alunoId,
                    CriadaEm = agora,
                    Aluno = await _context.Usuarios.FirstAsync(u => u.Id == alunoId)
                };
                _context.Avaliacoes.Add(avaliacao);
            }
            else
            {
                avaliacao.AtualizadaEm = agora;
            }

            avaliacao.Nota = nota;
            avaliacao.Comentario = comentario;
            await _context.SaveChangesAsync();
            return avaliacao;
        }

        public async Task<bool> Remover(int professorId, int alunoId)
        {
            var removidas = await _context.Avaliacoes
                .Where(a => a.ProfessorId == professorId && a.AlunoId == alunoId)
                .ExecuteDeleteAsync();
            return removidas > 0;
        }

        public async Task AtualizarResumoDoPerfil(int professorId)
        {
            var notas = _context.Avaliacoes.Where(a => a.ProfessorId == professorId);
            var total = await notas.CountAsync();
            var media = total == 0 ? 0m : Math.Round((decimal)await notas.AverageAsync(a => a.Nota), 2);

            await _context.PerfilProfessores
                .Where(p => p.UsuarioId == professorId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Avaliacao, media)
                    .SetProperty(p => p.TotalAvaliacoes, total));
        }
    }
};
