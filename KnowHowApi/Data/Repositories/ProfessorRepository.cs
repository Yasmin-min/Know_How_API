using KnowHowApi.Domain.Enum;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowHowApi.Data.Repositories
{
    public class ProfessorRepository : BaseRepository, IProfessorRepository
    {
        public ProfessorRepository(Context context) : base(context)
        {
        }

        public async Task<List<PerfilProfessor>> ListarProfessores()
        {
            return await _context.PerfilProfessores
                .Include(p => p.Usuario)
                .Where(p => p.Usuario.TipoUsuario == TipoUsuario.Professor)
                .ToListAsync();
        }
    }
};
