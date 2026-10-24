using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowHowApi.Data.Repositories
{
    public class ProfessorAlunoRepository : BaseRepository, IProfessorAlunoRepository
    {
        public ProfessorAlunoRepository(Context context) : base(context)
        {
        }

        // Alunos do professor: quem tem conversa com ele, quem tem aula agendada com ele
        // e quem ele adicionou manualmente (ProfessorAlunos).
        private IQueryable<Usuario> AlunosDoProfessor(int professorId)
        {
            var alunoIds = _context.Conversas
                .Where(c => c.ProfessorId == professorId)
                .Select(c => c.AlunoId)
                .Union(_context.AulasAgendadas
                    .Where(a => a.ProfessorId == professorId)
                    .Select(a => a.AlunoId))
                .Union(_context.ProfessorAlunos
                    .Where(p => p.ProfessorId == professorId)
                    .Select(p => p.AlunoId));

            return _context.Usuarios
                .AsNoTracking()
                .Where(u => u.TipoUsuario == TipoUsuario.Aluno && alunoIds.Contains(u.Id));
        }

        public async Task<List<Usuario>> ListarAlunosDoProfessor(int professorId)
        {
            return await AlunosDoProfessor(professorId)
                .OrderBy(u => u.Nome)
                .Select(u => new Usuario { Id = u.Id, Nome = u.Nome })
                .ToListAsync();
        }

        public async Task<Usuario?> GetAlunoDoProfessor(int professorId, int alunoId)
        {
            return await AlunosDoProfessor(professorId)
                .Where(u => u.Id == alunoId)
                .Select(u => new Usuario { Id = u.Id, Nome = u.Nome })
                .FirstOrDefaultAsync();
        }

        public async Task AdicionarAluno(int professorId, int alunoId)
        {
            var existe = await _context.ProfessorAlunos
                .AnyAsync(p => p.ProfessorId == professorId && p.AlunoId == alunoId);
            if (existe)
                return;

            var vinculo = new ProfessorAluno { ProfessorId = professorId, AlunoId = alunoId, CriadoEm = DateTime.UtcNow };
            _context.ProfessorAlunos.Add(vinculo);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Outra requisição criou o mesmo vínculo entre a checagem e o insert (chave única).
                _context.Entry(vinculo).State = EntityState.Detached;
                var criadoPorOutraRequisicao = await _context.ProfessorAlunos
                    .AnyAsync(p => p.ProfessorId == professorId && p.AlunoId == alunoId);
                if (!criadoPorOutraRequisicao)
                    throw;
            }
        }

        public async Task<NotaAluno?> GetNota(int professorId, int alunoId)
        {
            return await _context.NotasAluno
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.ProfessorId == professorId && n.AlunoId == alunoId);
        }

        public async Task<NotaAluno> SalvarNota(int professorId, int alunoId, string texto, DateTime atualizadaEm)
        {
            var nota = await _context.NotasAluno
                .FirstOrDefaultAsync(n => n.ProfessorId == professorId && n.AlunoId == alunoId);

            if (nota == null)
            {
                nota = new NotaAluno { ProfessorId = professorId, AlunoId = alunoId };
                _context.NotasAluno.Add(nota);
            }

            nota.Texto = texto;
            nota.AtualizadaEm = atualizadaEm;
            await _context.SaveChangesAsync();
            return nota;
        }

        public async Task RemoverNota(int professorId, int alunoId)
        {
            await _context.NotasAluno
                .Where(n => n.ProfessorId == professorId && n.AlunoId == alunoId)
                .ExecuteDeleteAsync();
        }
    }
};
