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

        public async Task<ProfessorDashboardResponseDTO?> ObterDashboard(int usuarioId)
        {
            var perfil = await _professorRepository.GetPerfilProfessorByUsuarioId(usuarioId);
            if (perfil == null)
                return null;

            return new ProfessorDashboardResponseDTO
            {
                Especialidade = perfil.Materia,
                Indicadores = new ProfessorDashboardIndicadoresDTO
                {
                    AulasAtivas = 0,
                    Alunos = 0,
                    AvaliacaoMedia = perfil.Avaliacao,
                    AulasSemana = 0
                },
                ProximasAulas = new List<ProfessorDashboardProximaAulaDTO>(),
                AtividadesRecentes = new List<ProfessorDashboardAtividadeDTO>(),
                MinhasAulas = new List<ProfessorAulaResponseDTO>()
            };
        }
    }
}
