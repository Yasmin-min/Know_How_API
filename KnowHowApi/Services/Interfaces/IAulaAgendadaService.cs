using KnowHowApi.Domain.DTOs;

namespace KnowHowApi.Services.Interfaces
{
    public interface IAulaAgendadaService
    {
        Task<List<AulaAgendadaDTO>> ListarAulas(int professorId, DateTime? inicio, DateTime? fim);
        Task<List<AulaAgendadaAlunoDTO>> ListarAulasDoAluno(int alunoId, DateTime? inicio, DateTime? fim);
        Task<AulaAgendadaDTO> CriarAula(int professorId, CriarAulaAgendadaRequestDTO request);
        Task<AulaAgendadaDTO> EditarAula(int professorId, Guid aulaId, EditarAulaAgendadaRequestDTO request);
        Task<AulaAgendadaDTO> CancelarAula(int professorId, Guid aulaId);
    }
}
