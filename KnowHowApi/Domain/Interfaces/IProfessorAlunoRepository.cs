using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IProfessorAlunoRepository
    {
        Task<List<Usuario>> ListarAlunosDoProfessor(int professorId);
        Task<Usuario?> GetAlunoDoProfessor(int professorId, int alunoId);
        Task AdicionarAluno(int professorId, int alunoId);
        Task<NotaAluno?> GetNota(int professorId, int alunoId);
        Task<NotaAluno> SalvarNota(int professorId, int alunoId, string texto, DateTime atualizadaEm);
        Task RemoverNota(int professorId, int alunoId);
    }
};
