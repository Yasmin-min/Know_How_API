using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IProfessorRepository
    {
        Task<List<PerfilProfessor>> ListarProfessores();
        Task<PerfilProfessor> CriarPerfilProfessor(PerfilProfessor perfil);
        Task<PerfilProfessor?> GetPerfilProfessorByUsuarioId(int usuarioId);
    }
};
