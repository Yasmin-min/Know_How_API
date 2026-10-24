using KnowHowApi.Domain.DTOs;

namespace KnowHowApi.Services.Interfaces
{
    public interface IProfessorService
    {
        Task<List<ProfessorDTO>> ListarProfessores();
        Task<ProfessorDTO?> ObterProfessorPublico(int professorId);
        Task<ProfessorDashboardResponseDTO?> ObterDashboard(int usuarioId);
        Task<ProfessorAulaResponseDTO> CriarAula(int professorId, CriarAulaRequestDTO request);
        Task<List<ProfessorAulaResponseDTO>> ListarAulas(int professorId);
        Task<ProfessorAulaResponseDTO> ObterAula(int professorId, int aulaId);
        Task<ProfessorAulaResponseDTO> AtualizarAula(int professorId, int aulaId, CriarAulaRequestDTO request);
        Task<ProfessorAulaResponseDTO> AlterarStatusAula(int professorId, int aulaId, AlterarStatusAulaRequestDTO request);
        Task<ProfessorPerfilResponseDTO?> ObterPerfil(int usuarioId);
        Task<ProfessorPerfilResponseDTO?> AtualizarPerfil(int usuarioId, ProfessorPerfilRequestDTO request);
    }
}
