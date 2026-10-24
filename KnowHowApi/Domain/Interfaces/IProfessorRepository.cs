using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IProfessorRepository
    {
        Task<List<PerfilProfessor>> ListarProfessores();
        // Dados públicos do professor, sem as fotos (bytea); null se não existir.
        Task<PerfilProfessor?> GetPerfilPublico(int usuarioId);
        Task<PerfilProfessor> CriarPerfilProfessor(PerfilProfessor perfil);
        Task<PerfilProfessor?> GetPerfilProfessorByUsuarioId(int usuarioId);
        Task<PerfilProfessor> AtualizarPerfilProfessor(PerfilProfessor perfil);
    }
};
