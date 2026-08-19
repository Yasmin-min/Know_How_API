namespace KnowHowApi.Domain.DTOs
{
    public class SolicitarRecuperacaoSenhaResponseDTO
    {
        public string Mensagem { get; set; }

        public SolicitarRecuperacaoSenhaResponseDTO()
        {
        }

        public SolicitarRecuperacaoSenhaResponseDTO(string mensagem)
        {
            Mensagem = mensagem;
        }
    }
}
