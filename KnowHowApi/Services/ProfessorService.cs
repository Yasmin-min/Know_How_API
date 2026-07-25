using KnowHowApi.Domain.DTOs;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Services.Interfaces;

namespace KnowHowApi.Services
{
    public class ProfessorService : IProfessorService
    {
        private readonly IProfessorRepository _professorRepository;

        public ProfessorService(IProfessorRepository professorRepository)
        {
            _professorRepository = professorRepository;
        }

        public async Task<List<ProfessorDTO>> ListarProfessores()
        {
            var perfis = await _professorRepository.ListarProfessores();
            return perfis.Select(p => new ProfessorDTO(p)).ToList();
        }
    }
}
