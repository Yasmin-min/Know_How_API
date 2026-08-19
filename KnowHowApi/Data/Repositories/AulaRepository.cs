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

        public async Task<bool> ExisteAulaComTitulo(int professorId, string titulo)
        {
            return await _context.Aulas
                .AsNoTracking()
                .AnyAsync(a => a.ProfessorId == professorId && a.Titulo == titulo);
        }
    }
};
