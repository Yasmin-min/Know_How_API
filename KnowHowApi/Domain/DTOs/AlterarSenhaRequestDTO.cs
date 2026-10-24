namespace KnowHowApi.Domain.DTOs
{
    public class AlterarSenhaRequestDTO
    {
        public string SenhaAtual { get; set; } = string.Empty;
        public string NovaSenha { get; set; } = string.Empty;
    }
}
