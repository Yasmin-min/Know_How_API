using KnowHowApi.Domain.DTOs;

namespace KnowHowApi.Services.Interfaces
{
    public interface IAvaliacaoService
    {
        // alunoLogadoId preenche MinhaAvaliacao; null quando quem consulta é professor.
        Task<AvaliacoesProfessorDTO> ListarDoProfessor(int professorId, int? alunoLogadoId);
        Task<AvaliacaoDTO> Salvar(int alunoId, int professorId, SalvarAvaliacaoRequestDTO request);
        Task Excluir(int alunoId, int professorId);
    }
}
