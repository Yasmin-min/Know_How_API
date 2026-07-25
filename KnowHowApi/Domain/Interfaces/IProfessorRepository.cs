using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IProfessorRepository
    {
        Task<List<PerfilProfessor>> ListarProfessores();
    }
};
