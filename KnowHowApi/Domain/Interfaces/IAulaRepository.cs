using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IAulaRepository
    {
        Task<Aula> CriarAula(Aula aula);
        Task<bool> ExisteAulaComTitulo(int professorId, string titulo);
    }
};
