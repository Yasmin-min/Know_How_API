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

        // Projeta só o que o perfil público usa, para não carregar as fotos (bytea) de cada professor.
        private static IQueryable<PerfilProfessor> ProjetarPublico(IQueryable<PerfilProfessor> query)
        {
            return query.Select(p => new PerfilProfessor
            {
                Id = p.Id,
                UsuarioId = p.UsuarioId,
                Usuario = new Usuario { Id = p.Usuario.Id, Nome = p.Usuario.Nome },
                Materia = p.Materia,
                ValorHora = p.ValorHora,
                Avaliacao = p.Avaliacao,
                TotalAvaliacoes = p.TotalAvaliacoes,
                Descricao = p.Descricao,
                Disponivel = p.Disponivel,
                AvatarVariante = p.AvatarVariante
            });
        }

        public async Task<List<PerfilProfessor>> ListarProfessores()
        {
            var query = _context.PerfilProfessores
                .AsNoTracking()
                .Where(p => p.Usuario.TipoUsuario == TipoUsuario.Professor);

            return await ProjetarPublico(query).ToListAsync();
        }

        public async Task<PerfilProfessor?> GetPerfilPublico(int usuarioId)
        {
            var query = _context.PerfilProfessores
                .AsNoTracking()
                .Where(p => p.UsuarioId == usuarioId && p.Usuario.TipoUsuario == TipoUsuario.Professor);

            return await ProjetarPublico(query).FirstOrDefaultAsync();
        }

        public async Task<PerfilProfessor> CriarPerfilProfessor(PerfilProfessor perfil)
        {
            _context.PerfilProfessores.Add(perfil);
            await _context.SaveChangesAsync();
            return perfil;
        }

        public async Task<PerfilProfessor?> GetPerfilProfessorByUsuarioId(int usuarioId)
        {
            return await _context.PerfilProfessores
                .Include(p => p.Usuario)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
        }

        public async Task<PerfilProfessor> AtualizarPerfilProfessor(PerfilProfessor perfil)
        {
            _context.PerfilProfessores.Update(perfil);
            await _context.SaveChangesAsync();
            return perfil;
        }
    }
};
