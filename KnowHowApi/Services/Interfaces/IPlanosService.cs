using KnowHowApi.Domain.DTOs;

namespace KnowHowApi.Services.Interfaces
{
    public interface IPlanosService
    {
        Task<ConfiguracaoPrecosDTO> ObterPrecos(int professorId);
        Task<ConfiguracaoPrecosDTO> AtualizarPrecos(int professorId, AtualizarPrecosRequestDTO request);

        Task<List<PacoteDTO>> ListarPacotes(int professorId);
        Task<PacoteDTO> CriarPacote(int professorId, SalvarPacoteRequestDTO request);
        Task<PacoteDTO> AtualizarPacote(int professorId, Guid pacoteId, SalvarPacoteRequestDTO request);
        Task ExcluirPacote(int professorId, Guid pacoteId);

        Task<List<PlanoMensalDTO>> ListarPlanosMensais(int professorId);
        Task<PlanoMensalDTO> CriarPlanoMensal(int professorId, SalvarPlanoMensalRequestDTO request);
        Task<PlanoMensalDTO> AtualizarPlanoMensal(int professorId, Guid planoId, SalvarPlanoMensalRequestDTO request);
        Task ExcluirPlanoMensal(int professorId, Guid planoId);

        Task<OfertasProfessorDTO> ObterOfertas(int professorId);

        Task<List<ContratacaoDTO>> ListarContratacoes(int professorId);
        Task<ContratacaoDTO> CriarContratacao(int professorId, CriarContratacaoRequestDTO request);
        Task<ContratacaoDTO> AlterarPagamento(int professorId, Guid contratacaoId, AlterarPagamentoContratacaoRequestDTO request);
        Task<ContratacaoDTO> AlterarAulasUsadas(int professorId, Guid contratacaoId, AlterarAulasUsadasRequestDTO request);
        Task ExcluirContratacao(int professorId, Guid contratacaoId);
    }
}
