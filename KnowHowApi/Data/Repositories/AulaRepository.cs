using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowHowApi.Data.Repositories
{
    public class AulaRepository : BaseRepository, IAulaRepository
    {
        public AulaRepository(Context context) : base(context)
        {
        }

        public async Task<Aula> CriarAula(Aula aula)
        {
            _context.Aulas.Add(aula);
            await _context.SaveChangesAsync();
            return aula;
        }

        public async Task<bool> ExisteAulaComTitulo(int professorId, string titulo, int? ignorarAulaId)
        {
            return await _context.Aulas
                .AsNoTracking()
                .AnyAsync(a => a.ProfessorId == professorId && a.Titulo == titulo && a.Id != ignorarAulaId);
        }

        public async Task<List<Aula>> ListarDoProfessor(int professorId)
        {
            return await _context.Aulas
                .AsNoTracking()
                .Include(a => a.AreaInteresse)
                .Where(a => a.ProfessorId == professorId)
                .OrderByDescending(a => a.CriadaEm)
                .ToListAsync();
        }

        public async Task<Aula?> GetAulaDoProfessor(int aulaId, int professorId)
        {
            return await _context.Aulas
                .AsNoTracking()
                .Include(a => a.AreaInteresse)
                .FirstOrDefaultAsync(a => a.Id == aulaId && a.ProfessorId == professorId);
        }

        public async Task<Aula> AtualizarAula(Aula aula)
        {
            // A aula vem de uma consulta AsNoTracking; a área de interesse é gravada só pelo
            // AreaInteresseId, para não anexar ao contexto a entidade da consulta anterior.
            var areaInteresse = aula.AreaInteresse;
            aula.AreaInteresse = null;

            _context.Aulas.Update(aula);
            await _context.SaveChangesAsync();

            _context.Entry(aula).State = EntityState.Detached;
            aula.AreaInteresse = areaInteresse;
            return aula;
        }

        // Uma única consulta para todos os professores (evita N+1 em GET /Professor/listar).
        public async Task<List<Aula>> ListarAtivasDosProfessores(IEnumerable<int> professorIds)
        {
            var ids = professorIds.ToList();
            return await _context.Aulas
                .AsNoTracking()
                .Include(a => a.AreaInteresse)
                .Where(a => ids.Contains(a.ProfessorId) && a.Status == "ativa")
                .OrderByDescending(a => a.CriadaEm)
                .ToListAsync();
        }
    }
};
