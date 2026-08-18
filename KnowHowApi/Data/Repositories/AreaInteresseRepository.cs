using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowHowApi.Data.Repositories
{
    public class AreaInteresseRepository : BaseRepository, IAreaInteresseRepository
    {
        public AreaInteresseRepository(Context context) : base(context)
        {
        }

        public async Task<AreaInteresse?> GetAreaInteresseByNome(string nome)
        {
            return await _context.AreasInteresse
            .FirstOrDefaultAsync(a => a.Nome == nome);
        }
    }
};
