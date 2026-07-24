using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowHowApi.Data.Repositories
{
    public class UsuarioRepository : BaseRepository, IUsuarioRepository
    {
        public UsuarioRepository(Context context) : base(context)
        {
        }

        public async Task<Usuario?> GetUsuarioByEmail(string email)
        {
            return await _context.Usuarios
            .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Usuario?> GetUsuarioById(int id)
        {
            return await _context.Usuarios
            .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Usuario> CriarUsuario(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }
        public async Task<List<Usuario>> GetAllUsers()
        {
            return await _context.Usuarios.ToListAsync();
        }
        public async Task<Usuario> Update(Usuario userEdit)
        {
            _context.Usuarios.Update(userEdit);
            await _context.SaveChangesAsync();
            return userEdit;  

        }
    }
};