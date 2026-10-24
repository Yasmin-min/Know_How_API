namespace KnowHowApi.Domain.DTOs
{
    // GET /Professor/{professorId}/ofertas: o que o aluno vê no perfil do professor.
    // Somente pacotes e planos ativos.
    public class OfertasProfessorDTO
    {
        public ConfiguracaoPrecosDTO Precos { get; set; }
        public List<PacoteDTO> Pacotes { get; set; } = new();
        public List<PlanoMensalDTO> PlanosMensais { get; set; } = new();
    }
}
