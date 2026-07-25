using KnowHowApi.Domain.DTOs;

namespace KnowHowApi.Services.Interfaces
{
    public interface IProfessorService
    {
        Task<List<ProfessorDTO>> ListarProfessores();
    }
}
