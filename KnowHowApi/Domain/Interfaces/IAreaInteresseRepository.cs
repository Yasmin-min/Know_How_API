using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IAreaInteresseRepository
    {
        Task<AreaInteresse?> GetAreaInteresseByNome(string nome);
    }
};
