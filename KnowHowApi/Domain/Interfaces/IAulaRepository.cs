using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IAulaRepository
    {
        Task<Aula> CriarAula(Aula aula);
        Task<bool> ExisteAulaComTitulo(int professorId, string titulo, int? ignorarAulaId);
        Task<List<Aula>> ListarDoProfessor(int professorId);
        Task<Aula?> GetAulaDoProfessor(int aulaId, int professorId);
        Task<Aula> AtualizarAula(Aula aula);
        Task<List<Aula>> ListarAtivasDosProfessores(IEnumerable<int> professorIds);
    }
};
