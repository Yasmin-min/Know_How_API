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

        // Os e-mails são gravados como o usuário digitou; aqui a comparação ignora maiúsculas.
        public async Task<Usuario?> GetUsuarioByEmailIgnorandoCaixa(string emailMinusculo)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Email.Trim().ToLower() == emailMinusculo)
                .Select(u => new Usuario { Id = u.Id, Nome = u.Nome, Email = u.Email, TipoUsuario = u.TipoUsuario })
                .FirstOrDefaultAsync();
        }

        public async Task<Usuario?> GetUsuarioByCpf(string cpf)
        {
            return await _context.Usuarios
            .FirstOrDefaultAsync(c => c.Cpf == cpf);
        }

        public async Task<Usuario?> GetUsuarioById(int id)
        {
            return await _context.Usuarios
            .Include(c => c.AreaInteresse)
            .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Usuario> CriarUsuario(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }
        public async Task<Usuario> Update(Usuario userEdit)
        {
            _context.Usuarios.Update(userEdit);
            await _context.SaveChangesAsync();
            return userEdit;  

        }
    }
};