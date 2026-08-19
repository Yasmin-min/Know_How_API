namespace KnowHowApi.Domain.DTOs
{
    public class ConfirmarCodigoRecuperacaoResponseDTO
    {
        public string TokenRedefinicao { get; set; }

        public ConfirmarCodigoRecuperacaoResponseDTO()
        {
        }

        public ConfirmarCodigoRecuperacaoResponseDTO(string tokenRedefinicao)
        {
            TokenRedefinicao = tokenRedefinicao;
        }
    }
}
