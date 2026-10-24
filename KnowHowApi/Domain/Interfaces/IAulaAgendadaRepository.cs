using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IAulaAgendadaRepository
    {
        Task<List<AulaAgendada>> ListarDoProfessor(int professorId, DateTime? inicio, DateTime? fim);
        Task<List<AulaAgendada>> ListarDoAluno(int alunoId, DateTime? inicio, DateTime? fim);
        Task<AulaAgendada?> GetConflitoProfessor(int professorId, DateTime inicio, DateTime fim, Guid? ignorarAulaId = null);
        Task<bool> ExisteConflitoAluno(int alunoId, DateTime inicio, DateTime fim, Guid? ignorarAulaId = null);
        Task<AulaAgendada?> GetByIdDoProfessor(Guid id, int professorId);
        // Com os nomes do professor e do aluno; null se não existir.
        Task<AulaAgendada?> GetByIdComParticipantes(Guid id);
        Task<AulaAgendada> Criar(AulaAgendada aula);
        Task Atualizar(Guid id, int? aulaId, string titulo, string descricao, DateTime inicio, DateTime fim, int duracaoMinutos);
        Task Cancelar(Guid id, DateTime canceladaEm);
        Task<int> ContarAlunosAtendidos(int professorId, DateTime ate);
    }
};
