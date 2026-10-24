using KnowHowApi.Domain.DTOs;

namespace KnowHowApi.Services.Interfaces
{
    public interface IProfessorAlunoService
    {
        Task<List<AlunoResumoDTO>> ListarAlunos(int professorId);
        Task<AlunoResumoDTO> AdicionarAlunoPorEmail(int professorId, AdicionarAlunoRequestDTO request);
        Task<NotaAlunoDTO> ObterNota(int professorId, int alunoId);
        Task<NotaAlunoDTO> SalvarNota(int professorId, int alunoId, SalvarNotaAlunoRequestDTO request);
    }
}
