namespace KnowHowApi.Domain.DTOs
{
    public class RedefinirSenhaResponseDTO
    {
        public string Mensagem { get; set; }

        public RedefinirSenhaResponseDTO()
        {
        }

        public RedefinirSenhaResponseDTO(string mensagem)
        {
            Mensagem = mensagem;
        }
    }
}
