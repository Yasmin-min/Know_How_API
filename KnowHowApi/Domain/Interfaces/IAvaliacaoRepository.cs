using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IAvaliacaoRepository
    {
        Task<bool> ProfessorExiste(int professorId);
        Task<bool> AlunoTeveAulaComProfessor(int professorId, int alunoId, DateTime agora);
        // Com o nome do aluno preenchido em Aluno.Nome.
        Task<List<Avaliacao>> ListarPorProfessor(int professorId);
        Task<Avaliacao> Salvar(int professorId, int alunoId, int nota, string comentario, DateTime agora);
        Task<bool> Remover(int professorId, int alunoId);
        // Recalcula PerfilProfessor.Avaliacao/TotalAvaliacoes, usados em GET /Professor/listar e no dashboard.
        Task AtualizarResumoDoPerfil(int professorId);
    }
};
