using KnowHowApi.Domain.Models;

namespace KnowHowApi.Domain.Interfaces
{
    public interface IPlanosRepository
    {
        Task<ConfiguracaoPrecos?> GetConfiguracaoPrecos(int professorId);
        Task SalvarConfiguracaoPrecos(ConfiguracaoPrecos configuracao, decimal valorHora);

        Task<List<Pacote>> ListarPacotes(int professorId, bool somenteAtivos);
        Task<Pacote?> GetPacote(Guid id, int professorId);
        Task<Pacote> CriarPacote(Pacote pacote);
        Task<Pacote> AtualizarPacote(Pacote pacote);
        Task<bool> ExcluirPacote(Guid id, int professorId);

        Task<List<PlanoMensal>> ListarPlanosMensais(int professorId, bool somenteAtivos);
        Task<PlanoMensal?> GetPlanoMensal(Guid id, int professorId);
        Task<PlanoMensal> CriarPlanoMensal(PlanoMensal plano);
        Task<PlanoMensal> AtualizarPlanoMensal(PlanoMensal plano);
        Task<bool> ExcluirPlanoMensal(Guid id, int professorId);

        Task<List<Contratacao>> ListarContratacoes(int professorId);
        Task<Contratacao?> GetContratacao(Guid id, int professorId);
        Task<Contratacao> CriarContratacao(Contratacao contratacao);
        Task<Contratacao> AtualizarContratacao(Contratacao contratacao);
        Task<bool> ExcluirContratacao(Guid id, int professorId);
    }
};
