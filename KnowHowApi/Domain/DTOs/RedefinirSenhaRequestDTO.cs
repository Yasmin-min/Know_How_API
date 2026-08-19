namespace KnowHowApi.Domain.DTOs
{
    public class RedefinirSenhaRequestDTO
    {
        public string Email { get; set; }
        public string TokenRedefinicao { get; set; }
        public string NovaSenha { get; set; }
    }
}
